using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows;


namespace SEWorkbenchHelper
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var highlighting = HighlightingManager.Instance.GetDefinition("C#");
            CodeEditor.SyntaxHighlighting = highlighting;
        }
    }
}