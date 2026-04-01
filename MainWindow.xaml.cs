using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace SEWorkbenchHelper
{
    public partial class MainWindow : Window
    {
        private string currentFilePath = "";

        public MainWindow()
        {
            InitializeComponent();

        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "C# files (*.cs)|*.cs|text files (*.txt)|*.txt|all files (*.*)|*.*";
            
            if (openFileDialog.ShowDialog() == true)
            {
                currentFilePath = openFileDialog.FileName;
                textEditor.Load(currentFilePath);
                this.Title = "SEWorkbenchHelper - " + currentFilePath;
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "C# files (*.cs)|*.cs|text files (*.txt)|*.txt";

                if (saveFileDialog.ShowDialog() == true)
                {
                    currentFilePath = saveFileDialog.FileName;
                    this.Title = "SEWorkbenchHelper - " + currentFilePath;
                } else
                {
                    return;
                }
            }
            textEditor.Save(currentFilePath);
        }
    }
}