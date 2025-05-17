using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SEWorkbenchHelper
{
    public partial class MainWindow : Window
    {
        private readonly string _scriptsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

        public MainWindow()
        {
            InitializeComponent();
            LoadFileTree();
            CodeEditor.TextChanged += CodeEditor_TextChanged;
        }

        // Class for file tree items
        public class FileTreeItem
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public bool IsModified { get; set; }
            public BitmapImage Icon => IsDirectory ?
                new BitmapImage(new Uri("pack://application:,,,/Resources/folder.png")) :
                new BitmapImage(new Uri("pack://application:,,,/Resources/file.png"));
            public bool IsDirectory { get; set; }
            public ObservableCollection<FileTreeItem> SubItems { get; set; } = new ObservableCollection<FileTreeItem>();
        }

        // Load files into tree view
        private void LoadFileTree()
        {
            FilesTreeView.Items.Clear();

            if (!Directory.Exists(_scriptsFolder))
                Directory.CreateDirectory(_scriptsFolder);

            var rootItem = new FileTreeItem
            {
                Name = "Scripts",
                FullPath = _scriptsFolder,
                IsDirectory = true
            };

            foreach (var file in Directory.GetFiles(_scriptsFolder, "*.cs"))
            {
                rootItem.SubItems.Add(new FileTreeItem
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    IsDirectory = false
                });
            }

            FilesTreeView.Items.Add(rootItem);
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
                CodeEditor.Text = File.ReadAllText(selectedItem.FullPath);
                selectedItem.IsModified = false;
            }
        }
    }
}