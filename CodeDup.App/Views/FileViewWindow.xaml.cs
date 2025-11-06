using System.IO;
using System.Windows;
using CodeDup.Core.Models;
using CodeDup.Core.Storage;
using ICSharpCode.AvalonEdit.Highlighting;

namespace CodeDup.App.Views;

public partial class FileViewWindow : Window {
    private readonly CodeFileMetadata _file;
    private readonly string _project;
    private readonly IProjectStore _store;

    public FileViewWindow(string project, CodeFileMetadata file, IProjectStore store) {
        InitializeComponent();
        _project = project;
        _file = file;
        _store = store;

        LoadFileContent();
    }

    private void LoadFileContent() {
        try {
            // 设置文件信息
            FileNameLabel.Text = _file.FileName;
            LanguageLabel.Text = _file.ProgrammingLanguage;
            SizeLabel.Text = $"{_file.FileSizeBytes:N0} 字节";
            ImportedAtLabel.Text = _file.ImportedAt.ToString("yyyy-MM-dd HH:mm:ss");

            // 加载文件原始内容
            var filePath = _store.GetFileContentPath(_project, _file.Id);
            
            if (File.Exists(filePath)) {
                var content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                
                // 根据文件扩展名设置语法高亮
                var syntaxHighlighting = GetSyntaxHighlighting(_file.Extension);
                if (syntaxHighlighting != null) {
                    FileContentBox.SyntaxHighlighting = syntaxHighlighting;
                }
                
                FileContentBox.Text = content;
            } else {
                FileContentBox.Text = "文件不存在或已被删除";
            }
        }
        catch (Exception ex) {
            MessageBox.Show($"加载文件内容时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private IHighlightingDefinition? GetSyntaxHighlighting(string extension) {
        return extension.ToLower() switch {
            "cs" => HighlightingManager.Instance.GetDefinition("C#"),
            "py" => HighlightingManager.Instance.GetDefinition("Python"),
            "html" or "htm" => HighlightingManager.Instance.GetDefinition("HTML"),
            "xml" => HighlightingManager.Instance.GetDefinition("XML"),
            "js" => HighlightingManager.Instance.GetDefinition("JavaScript"),
            "cpp" or "cc" or "cxx" => HighlightingManager.Instance.GetDefinition("C++"),
            "c" or "h" or "hpp" => HighlightingManager.Instance.GetDefinition("C++"),
            "java" => HighlightingManager.Instance.GetDefinition("Java"),
            "sql" => HighlightingManager.Instance.GetDefinition("SQL"),
            "php" => HighlightingManager.Instance.GetDefinition("PHP"),
            "vb" => HighlightingManager.Instance.GetDefinition("VB"),
            "json" => HighlightingManager.Instance.GetDefinition("JavaScript"), // JSON 使用 JavaScript 高亮
            "css" => HighlightingManager.Instance.GetDefinition("CSS"),
            _ => null // 纯文本，无高亮
        };
    }

    private void Close_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}
