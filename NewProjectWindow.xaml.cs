using System.IO;
using System.Windows;
using System.Xml;
using Newtonsoft.Json;
using static SEWorkbenchHelper.MainWindow;

namespace SEWorkbenchHelper
{
    public partial class NewProjectWindow : Window
    {
        public string ProjectPath { get; private set; }

        public NewProjectWindow()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
            {
                MessageBox.Show("Please enter project name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var project = new Project
            {
                Name = ProjectNameTextBox.Text,
                Author = AuthorTextBox.Text,
                Description = DescriptionTextBox.Text
            };

            // Создаем папку проекта
            string projectsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Projects");
            if (!Directory.Exists(projectsFolder))
                Directory.CreateDirectory(projectsFolder);

            ProjectPath = Path.Combine(projectsFolder, ProjectNameTextBox.Text);
            Directory.CreateDirectory(ProjectPath);

            // Создаем файл проекта
            string projectJson = JsonConvert.SerializeObject(project, (Newtonsoft.Json.Formatting)System.Xml.Formatting.Indented);
            File.WriteAllText(Path.Combine(ProjectPath, "project.json"), projectJson);

            // Создаем стандартную структуру папок
            Directory.CreateDirectory(Path.Combine(ProjectPath, "Scripts"));
            Directory.CreateDirectory(Path.Combine(ProjectPath, "Data"));
            Directory.CreateDirectory(Path.Combine(ProjectPath, "Resources"));

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}