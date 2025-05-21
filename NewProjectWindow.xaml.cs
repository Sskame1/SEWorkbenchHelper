using System;
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
                MessageBox.Show("Specify the project name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var project = new Project
            {
                Name = ProjectNameTextBox.Text,
                Author = AuthorTextBox.Text,
                Description = DescriptionTextBox.Text
            };

            string projectPath = Path.Combine(Directory.GetCurrentDirectory(), "Projects", ProjectNameTextBox.Text);

            try
            {
                Directory.CreateDirectory(projectPath);
                File.WriteAllText(
                    Path.Combine(projectPath, "project.json"),
                    JsonConvert.SerializeObject(project, Newtonsoft.Json.Formatting.Indented)
                );

                Directory.CreateDirectory(Path.Combine(projectPath, "Scripts"));
                Directory.CreateDirectory(Path.Combine(projectPath, "Data"));

                ProjectPath = projectPath;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}