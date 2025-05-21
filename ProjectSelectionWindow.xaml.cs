using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using static SEWorkbenchHelper.MainWindow;

namespace SEWorkbenchHelper
{
    public partial class ProjectSelectionWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ProjectInfo> _projects;
        public ObservableCollection<ProjectInfo> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged(nameof(Projects));
            }
        }

        public class ProjectInfo
        {
            public string Name { get; set; }
            public string Author { get; set; }
            public string Description { get; set; }
            public string Version { get; set; }
            public List<string> Files { get; set; }
            public string Path { get; set; }
        }

        public ProjectSelectionWindow()
        {
            InitializeComponent();
            Projects = new ObservableCollection<ProjectInfo>();

            RefreshProjectsList();
            DataContext = this;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshProjectsList()
        {
            Projects.Clear();

            string projectsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Projects");
            if (!Directory.Exists(projectsFolder)) return;

            foreach (var dir in Directory.GetDirectories(projectsFolder))
            {
                string projectFile = Path.Combine(dir, "project.json");
                if (!File.Exists(projectFile)) continue;

                try
                {
                    var project = JsonConvert.DeserializeObject<Project>(File.ReadAllText(projectFile));
                    Projects.Add(new ProjectInfo
                    {
                        Name = project.Name,
                        Path = dir,
                        Description = project.Description,
                        Files = GetProjectFiles(dir)
                    });
                }
                catch { /* Ignore Error */ }
            }
        }

        private List<string> GetProjectFiles(string path)
        {
            var files = new List<string>();

            try
            {
                // Add directories
                foreach (var dir in Directory.GetDirectories(path))
                {
                    files.Add($"[{Path.GetFileName(dir)}]");
                }

                // Add files in root
                foreach (var file in Directory.GetFiles(path))
                {
                    if (Path.GetFileName(file) != "project.json")
                    {
                        files.Add(Path.GetFileName(file));
                    }
                }

                // Add example file from Scripts folder
                string scriptsPath = Path.Combine(path, "Scripts");
                if (Directory.Exists(scriptsPath) && Directory.GetFiles(scriptsPath).Length > 0)
                {
                    files.Add($"Scripts/{Path.GetFileName(Directory.GetFiles(scriptsPath)[0])}");
                }
            }
            catch { /* Ignore access errors */ }

            return files; // Show only first 5 items
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).Tag is ProjectInfo projectInfo)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();

                string scriptsPath = Path.Combine(projectInfo.Path, "Scripts");
                mainWindow.LoadProject(projectInfo.Path, scriptsPath);

                Close();
            }
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var newProjectWindow = new NewProjectWindow();
            newProjectWindow.Owner = this;
            if (newProjectWindow.ShowDialog() == true)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    RefreshProjectsList();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).Tag is ProjectInfo projectInfo)
            {
                var result = MessageBox.Show($"Are you sure you want to delete project '{projectInfo.Name}'?",
                                           "Confirm Delete",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.Delete(projectInfo.Path, true);

                        Projects.Remove(projectInfo);

                        MessageBox.Show("Project deleted successfully!", "Success",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting project: {ex.Message}", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}