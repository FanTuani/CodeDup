using System.IO;
using System.Text;
using System.Windows;
using CodeDup.Core.Models;
using CodeDup.Core.Storage;
using CodeDup.Text.Extractors;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace CodeDup.App.Views;

public partial class FileCompareWindow : Window {
    private readonly List<ITextExtractor> _extractors = new() {
        new TextExtractorDocx(),
        new TextExtractorPdf()
    };
    
    private readonly PairDisplayResult _pair;
    private readonly string _project;
    private readonly IProjectStore _store;
    private string _contentA = string.Empty;
    private string _contentB = string.Empty;
    private string _extensionA = string.Empty;
    private string _extensionB = string.Empty;

    public FileCompareWindow(string project, PairDisplayResult pair, IProjectStore store) {
        InitializeComponent();
        _project = project;
        _pair = pair;
        _store = store;

        LoadFileContents();
    }

    private void LoadFileContents() {
        try {
            // 设置标签
            FileALabel.Text = _pair.FileNameA;
            FileBLabel.Text = _pair.FileNameB;
            SimilarityLabel.Text = $"{_pair.Similarity:P2}";

            // 加载原始文件内容
            var fileAPath = _store.GetFileContentPath(_project, _pair.FileIdA);
            var fileBPath = _store.GetFileContentPath(_project, _pair.FileIdB);

            var extensionA = Path.GetExtension(_pair.FileNameA).TrimStart('.');
            var extensionB = Path.GetExtension(_pair.FileNameB).TrimStart('.');
            
            _extensionA = extensionA;
            _extensionB = extensionB;

            // 提取文件内容（支持 PDF、DOCX 等特殊格式）
            var rawContentA = ExtractFileContent(fileAPath, extensionA);
            var rawContentB = ExtractFileContent(fileBPath, extensionB);
            
            // 剔除注释后显示
            _contentA = Preprocess.StripCommentsAndNoise(rawContentA, extensionA);
            _contentB = Preprocess.StripCommentsAndNoise(rawContentB, extensionB);

            // 显示并排视图，高亮重复部分
            UpdateDiffView();
        }
        catch (Exception ex) {
            MessageBox.Show($"加载文件内容时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string ExtractFileContent(string filePath, string extension) {
        if (!File.Exists(filePath)) {
            return "文件不存在";
        }

        try {
            // 检查是否需要特殊提取器（PDF、DOCX 等）
            var handler = _extractors.FirstOrDefault(x => x.CanHandle(extension));
            if (handler != null) {
                return handler.ExtractText(filePath);
            }

            // 普通文本文件直接读取
            return File.ReadAllText(filePath, Encoding.UTF8);
        }
        catch {
            return "文件读取失败";
        }
    }

    private void UpdateDiffView() {
        // 使用并排模式
        var sideBySideBuilder = new SideBySideDiffBuilder(new DiffPlex.Differ());
        var sideBySideModel = sideBySideBuilder.BuildDiffModel(_contentA, _contentB);
        CustomDiffViewer.SetDiffModel(sideBySideModel, _extensionA, _extensionB);
    }

    private void Close_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}