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
    private bool _isSideBySideMode = false;
    private string _contentA = string.Empty;
    private string _contentB = string.Empty;

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
            
            _contentA = Preprocess.StripCommentsAndNoise(rawContentA, extensionA);
            _contentB = Preprocess.StripCommentsAndNoise(rawContentB, extensionB);

            // 初始显示内联模式
            UpdateDiffView();
        }
        catch (Exception ex) {
            MessageBox.Show($"加载文件内容时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateDiffView() {
        if (_isSideBySideMode) {
            // 并排模式
            var sideBySideBuilder = new SideBySideDiffBuilder(new DiffPlex.Differ());
            var sideBySideModel = sideBySideBuilder.BuildDiffModel(_contentA, _contentB);
            SideBySideDiffViewer.DiffModel = sideBySideModel;
        } else {
            // 内联模式
            var inlineBuilder = new InlineDiffBuilder(new DiffPlex.Differ());
            var inlineModel = inlineBuilder.BuildDiffModel(_contentA, _contentB);
            InlineDiffViewer.DiffModel = inlineModel;
        }
    }

    private void ToggleView_Click(object sender, RoutedEventArgs e) {
        _isSideBySideMode = !_isSideBySideMode;
        
        if (_isSideBySideMode) {
            // 切换到并排视图
            InlineDiffViewer.Visibility = Visibility.Collapsed;
            SideBySideDiffViewer.Visibility = Visibility.Visible;
            ToggleViewButton.Content = "切换为内联视图";
        } else {
            // 切换到内联视图
            SideBySideDiffViewer.Visibility = Visibility.Collapsed;
            InlineDiffViewer.Visibility = Visibility.Visible;
            ToggleViewButton.Content = "切换为并排视图";
        }
        
        UpdateDiffView();
    }

    private void Close_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}