using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Controls;

namespace SEWorkbenchHelper
{
    public partial class MainWindow : Window
    {
        private readonly string _scriptsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

        public class ScriptFile
        {
            public string FullPath { get; set; }
            public string FileName => System.IO.Path.GetFileName(FullPath);
        }
        public MainWindow()
        {
            InitializeComponent();
            LoadScriptList();
        }

        private void LoadScriptList ()
        {
            if (!Directory.Exists(_scriptsFolder)) Directory.CreateDirectory(_scriptsFolder);

            var scriptFiles = Directory.GetFiles(_scriptsFolder, "*.cs")
                .Select(f => new ScriptFile { FullPath = f })
                .ToList();

            FilesListView.ItemsSource = scriptFiles;
        }

        private void Refresh_Button(object sender, RoutedEventArgs e)
        {
            LoadScriptList();
        }

        private void Create_Button(object sender, RoutedEventArgs e)
        {
            string newFilePath = System.IO.Path.Combine(_scriptsFolder, "ExampleScript.cs");

            int counter = 1;

            while (File.Exists(newFilePath))
            {
                newFilePath = System.IO.Path.Combine(_scriptsFolder, $"ExampleScript{counter}.cs");
                counter++;
            }

            File.WriteAllText(newFilePath,
                "using Sandbox.ModAPI.Ingame;\n" +
                "using System;\n\n" +
                "public class Program : MyGridProgram\n" +
                "{\n    void Main(string argument)\n    {\n        // Ваш код здесь\n    }\n}");

            LoadScriptList();
        }

        private void Delete_Button(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string filePath)
            {
                var result = MessageBox.Show(
                    $"Delete Script {System.IO.Path.GetFileName(filePath)}?",
                    "Confirm delete script",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(filePath);
                    LoadScriptList();
                }
            }
        }

        private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilesListView.SelectedItem is string selectedFile)
            {
                CodeEditor.Load(selectedFile);
            }
        }
    }
}
