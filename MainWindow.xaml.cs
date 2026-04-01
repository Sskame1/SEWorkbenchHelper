using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;

namespace SEWorkbenchHelper
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AdhocWorkspace _workspace;
        private Document _document;

        public MainWindow()
        {
            InitializeComponent();
            InitializeRoslyn();

            CodeEditor.TextChanged += CodeEditor_TextChanged;

            var highlighting = HighlightingManager.Instance.GetDefinition("C#");
            CodeEditor.SyntaxHighlighting = highlighting;
        }

        private void InitializeRoslyn()
        {
            _workspace = new AdhocWorkspace();

            var projInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                "MyScript",
                "MyScript",
                LanguageNames.CSharp);

            var project = _workspace.AddProject(projInfo);

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.EditorAttribute).Assembly.Location),
            };

            project = project.AddMetadataReferences(references);

            _document = project.AddDocument("code.cs", "");
        }

        private void CodeEditor_TextChanged(object sender, EventArgs e)
        {
            var newText = CodeEditor.Text;
            _document = _document.WithText(SourceText.From(newText));
        }

        private async void Button_Click(object sender, EventArgs e)
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var root = await syntaxTree.GetRootAsync();

            MessageBox.Show($"Количество узлов в коде: {root.DescendantNodes().Count()}");
        }
    }
}