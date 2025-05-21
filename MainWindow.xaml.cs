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
        private string _currentProjectPath;

        public MainWindow()
        {
            InitializeComponent();

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
                    if (IsProjectRoot) return LoadIcon("project.png");
                    if (IsDirectory) return LoadIcon("folder.png");

                    string ext = Path.GetExtension(Name)?.ToLower();
                   switch (ext)
                    {
                        case ".cs":
                            return LoadIcon("cs.png");
                        case ".txt":
                            return LoadIcon("txt.png");
                        case ".json":
                            return LoadIcon("json.png");
                        default:
                            return LoadIcon("file.png");
                    };
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

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var newProjectWindow = new NewProjectWindow();
            newProjectWindow.Owner = this;
        }

        public void LoadProject(string projectPath, string currentScriptsFolder)
        {
            _currentProjectPath = projectPath;
            string projectJsonPath = Path.Combine(projectPath, "project.json");

            if (File.Exists(projectJsonPath))
            {
                try
                {
                    var project = JsonConvert.DeserializeObject<Project>(File.ReadAllText(projectJsonPath));

                    Title = $"SEWorkbenchHelper - {project.Name}";
                    LoadFileTree(projectPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading project: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Load files into tree view
        private void LoadFileTree(string rootPath)
        {
            FilesTreeView.Items.Clear();

            var rootItem = new FileTreeItem
            {
                Name = Path.GetFileName(rootPath),
                FullPath = rootPath,
                IsDirectory = true,
                IsProjectRoot = true
            };

            LoadDirectoryContents(rootItem, rootPath);
            FilesTreeView.Items.Add(rootItem);
        }
        private void LoadDirectoryContents(FileTreeItem parentItem, string directoryPath)
        {
            try
            {
                if (Path.GetFileName(directoryPath).StartsWith(".")) return;

                foreach (var file in Directory.GetFiles(directoryPath))
                {
                    if (Path.GetFileName(file).StartsWith("~$")) continue;

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
           if (string.IsNullOrEmpty(_currentProjectPath))
            {
                MessageBox.Show("No project opened", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = _currentProjectPath,
                Description = "Select folder to create new file"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string newFilePath = Path.Combine(dialog.SelectedPath, "NewFile.cs");
                int counter = 1;

                while (File.Exists(newFilePath))
                {
                    newFilePath = Path.Combine(dialog.SelectedPath, $"NewFile{counter}.cs");
                    counter++;
                }

                string defaultContent = "using Sandbox.ModAPI.Ingame;\n" +
                                      "using System;\n\n" +
                                      "public class Program : MyGridProgram\n" +
                                      "{\n    void Main(string argument)\n    {\n        // Your code here\n    }\n}";

                File.WriteAllText(newFilePath, defaultContent);
                LoadFileTree(_currentProjectPath);
            }
        }

        private void Refresh_Button(object sender, RoutedEventArgs e)
        {
            LoadFileTree(_currentProjectPath);
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