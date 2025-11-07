using System.IO;
using System.Windows;
using CodeDup.Core.Models;
using CodeDup.Core.Storage;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace CodeDup.App.Views;

public partial class FileCompareWindow : Window {
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

            var rawContentA = File.Exists(fileAPath) ? File.ReadAllText(fileAPath, System.Text.Encoding.UTF8) : "文件不存在";
            var rawContentB = File.Exists(fileBPath) ? File.ReadAllText(fileBPath, System.Text.Encoding.UTF8) : "文件不存在";

            // 剔除注释后显示
            var extensionA = Path.GetExtension(_pair.FileNameA).TrimStart('.');
            var extensionB = Path.GetExtension(_pair.FileNameB).TrimStart('.');
            
            _extensionA = extensionA;
            _extensionB = extensionB;
            
            _contentA = Preprocess.StripCommentsAndNoise(rawContentA, extensionA);
            _contentB = Preprocess.StripCommentsAndNoise(rawContentB, extensionB);

            // 显示并排视图，高亮重复部分
            UpdateDiffView();
        }
        catch (Exception ex) {
            MessageBox.Show($"加载文件内容时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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