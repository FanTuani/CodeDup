using System.IO;
using System.Windows;
using CodeDup.Core.Models;
using CodeDup.Core.Storage;

namespace CodeDup.App.Views;

public partial class FileCompareWindow : Window {
    private readonly PairDisplayResult _pair;
    private readonly string _project;
    private readonly IProjectStore _store;

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

            // 加载原始文件内容（不再使用处理后的文本）
            var fileAPath = _store.GetFileContentPath(_project, _pair.FileIdA);
            var fileBPath = _store.GetFileContentPath(_project, _pair.FileIdB);

            var contentA = File.Exists(fileAPath) ? File.ReadAllText(fileAPath, System.Text.Encoding.UTF8) : "文件不存在";
            var contentB = File.Exists(fileBPath) ? File.ReadAllText(fileBPath, System.Text.Encoding.UTF8) : "文件不存在";

            FileAText.Text = contentA;
            FileBText.Text = contentB;

            HighlightDifferences(); // TODO
        }
        catch (Exception ex) {
            MessageBox.Show($"加载文件内容时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void HighlightDifferences() {
        // TODO

        var linesA = FileAText.Text.Split('\n');
        var linesB = FileBText.Text.Split('\n');
    }

    private void Close_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}