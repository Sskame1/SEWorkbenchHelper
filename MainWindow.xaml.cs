using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace SEWorkbenchHelper
{
    public partial class MainWindow : Window
    {
        private readonly string _scriptsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

        public MainWindow()
        {
            InitializeComponent();

            string resourcesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
            }

            LoadFileTree();
            CodeEditor.TextChanged += CodeEditor_TextChanged;
            CodeEditor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(100, 173, 214, 255));
        }

        public class Project
        {
            public string Name { get; set; }
            public string Author { get; set; }
            public string Version { get; set; } = "1.0.0";
            public string Description { get; set; }
            public List<string> Files { get; set; } = new List<string>();
        }

        // Class for file tree items
        public class FileTreeItem
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public bool IsModified { get; set; }
            public bool IsDirectory {  get; set; }
            public bool IsProjectRoot { get; set; }
            public ObservableCollection<FileTreeItem> SubItems { get; set; } = new ObservableCollection<FileTreeItem>();
            public BitmapImage Icon
            {
                get
                {
                    if (IsProjectRoot)
                    {
                        return LoadIcon("project.png");
                    }
                    return IsDirectory ? LoadIcon("folder.png") : LoadIcon("file.png");
                }
            }

            private BitmapImage LoadIcon(string iconName)
            {
                try
                {
                    string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", iconName);
                    if (File.Exists(iconPath))
                    {
                        return new BitmapImage(new Uri(iconPath));
                    }
                    return new BitmapImage(new Uri($"pack://application:,,,/Resources/{iconName}"));
                }
                catch
                {
                    return null;
                }
            }
        }
            public ObservableCollection<FileTreeItem> SubItems { get; set; } = new ObservableCollection<FileTreeItem>();

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var newProjectWindow = new NewProjectWindow();
            if (newProjectWindow.ShowDialog() == true)
            {
                LoadProject(newProjectWindow.ProjectPath, Get_scriptsFolder());
            }
        }

        private string Get_scriptsFolder()
        {
            return _scriptsFolder;
        }

        private void LoadProject(string projectPath, string currentScriptsFolder)
        {
            string projectJsonPath = Path.Combine(projectPath, "project.json");
            if (File.Exists(projectJsonPath))
            {
                try
                {
                    string json = File.ReadAllText(projectJsonPath);
                    var project = JsonConvert.DeserializeObject<Project>(json);

                    Title = $"SEWorkbenchHelper - {project.Name}";
                    string newScriptsFolder = Path.Combine(projectPath, "Scripts");
                    LoadFileTree(newScriptsFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading project: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Load files into tree view
        private void LoadFileTree(string scriptsFolder = null)
        {
            string folderToLoad = scriptsFolder ?? _scriptsFolder;
            FilesTreeView.Items.Clear();

            if (!Directory.Exists(folderToLoad))
            {
                Directory.CreateDirectory(folderToLoad);
                return;
            }

            bool isProject = File.Exists(Path.Combine(Directory.GetParent(folderToLoad).FullName, "project.json"));

            var rootItem = new FileTreeItem
            {
                Name = isProject ? Path.GetFileName(Directory.GetParent(folderToLoad).FullName) : "Scripts",
                FullPath = folderToLoad,
                IsDirectory = true,
                IsProjectRoot = isProject
            };

            LoadDirectoryContents(rootItem, folderToLoad);
            FilesTreeView.Items.Add(rootItem);
        }
        private void LoadDirectoryContents(FileTreeItem parentItem, string directoryPath)
        {
            try
            {
                foreach (var file in Directory.GetFiles(directoryPath))
                {
                    parentItem.SubItems.Add(new FileTreeItem
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false
                    });
                }

                foreach (var dir in Directory.GetDirectories(directoryPath))
                {
                    var dirItem = new FileTreeItem
                    {
                        Name = Path.GetFileName(dir),
                        FullPath = dir,
                        IsDirectory = true
                    };

                    parentItem.SubItems.Add(dirItem);
                    LoadDirectoryContents(dirItem, dir);
                }
            }
            catch (UnauthorizedAccessException) { }
        }

        // Handle text changes in editor
        private void CodeEditor_TextChanged(object sender, EventArgs e)
        {
            if (FilesTreeView.SelectedItem is FileTreeItem selectedItem && !selectedItem.IsDirectory)
            {
                string savedText = File.Exists(selectedItem.FullPath) ?
                    File.ReadAllText(selectedItem.FullPath) : string.Empty;
                selectedItem.IsModified = CodeEditor.Text != savedText;
            }
        }

        // Button handlers
        private void Create_Button(object sender, RoutedEventArgs e)
        {
            string newFilePath = Path.Combine(_scriptsFolder, "NewScript.cs");
            int counter = 1;

            while (File.Exists(newFilePath))
            {
                newFilePath = Path.Combine(_scriptsFolder, $"NewScript{counter}.cs");
                counter++;
            }

            string defaultContent = "using Sandbox.ModAPI.Ingame;\n" +
                                  "using System;\n\n" +
                                  "public class Program : MyGridProgram\n" +
                                  "{\n    void Main(string argument)\n    {\n        // Your code here\n    }\n}";

            File.WriteAllText(newFilePath, defaultContent);
            LoadFileTree();
        }

        private void Refresh_Button(object sender, RoutedEventArgs e)
        {
            LoadFileTree();
        }

        private void Save_Button(object sender, RoutedEventArgs e)
        {
            if (FilesTreeView.SelectedItem is FileTreeItem selectedItem && !selectedItem.IsDirectory)
            {
                try
                {
                    File.WriteAllText(selectedItem.FullPath, CodeEditor.Text);
                    selectedItem.IsModified = false;
                    MessageBox.Show("File saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No file selected!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("It's not a working button yet.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Handle file selection in tree view
        private void FilesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (FilesTreeView.SelectedItem is FileTreeItem selectedItem && !selectedItem.IsDirectory)
            {
                try
                {
                    CodeEditor.Text = File.ReadAllText(selectedItem.FullPath, Encoding.UTF8);

                    string extension = Path.GetExtension(selectedItem.FullPath).ToLower();
                    var highlightingManager = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance;

                    switch (extension)
                    {
                        case ".cs":
                            CodeEditor.SyntaxHighlighting = highlightingManager.GetDefinition("C#");
                            break;
                        case ".txt":
                            CodeEditor.SyntaxHighlighting = null;
                            break;
                        default:
                            CodeEditor.SyntaxHighlighting = highlightingManager.GetDefinitionByExtension(extension);
                            break;
                    }

                    selectedItem.IsModified = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error when opening a file: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}