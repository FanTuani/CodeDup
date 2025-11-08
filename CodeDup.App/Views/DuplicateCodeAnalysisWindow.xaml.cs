using System.Windows;
using System.Windows.Controls;
using CodeDup.Core.Models;
using CodeDup.Core.Services;
using CodeDup.Core.Storage;

namespace CodeDup.App.Views;

public partial class DuplicateCodeAnalysisWindow : Window {
    private readonly string _project;
    private readonly List<string> _fileIds;
    private readonly List<CodeFileMetadata> _allFiles;
    private readonly IProjectStore _store;
    private DuplicateAnalysisResult? _analysisResult;

    public DuplicateCodeAnalysisWindow(
        string project, 
        List<string> fileIds, 
        List<CodeFileMetadata> allFiles,
        IProjectStore store) {
        InitializeComponent();
        _project = project;
        _fileIds = fileIds;
        _allFiles = allFiles;
        _store = store;
    }

    // 开始分析按钮点击事件
    private void Analyze_Click(object sender, RoutedEventArgs e) {
        if (!int.TryParse(MinOccurrencesBox.Text, out var minOccurrences) || minOccurrences < 2) {
            MessageBox.Show("最小重复次数必须 >= 2", "参数错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(MinLineCountBox.Text, out var minLineCount) || minLineCount < 1) {
            MessageBox.Show("最小代码行数必须 >= 1", "参数错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try {
            // 显示进度提示
            StatsText.Text = "分析中...";
            FragmentList.ItemsSource = null;
            
            // 执行分析
            var analyzer = new DuplicateCodeAnalyzer(_store);
            _analysisResult = analyzer.AnalyzeDuplicateCode(
                _project, 
                _fileIds, 
                _allFiles, 
                minOccurrences, 
                minLineCount);

            // 显示结果
            StatsText.Text = $"找到 {_analysisResult.TotalFragments} 个重复片段，" +
                           $"总共 {_analysisResult.TotalOccurrences} 次重复";
            FragmentList.ItemsSource = _analysisResult.Fragments;

            if (_analysisResult.TotalFragments == 0) {
                MessageBox.Show("未找到符合条件的重复代码", "分析完成", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex) {
            MessageBox.Show($"分析失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // 选中重复片段时显示详细信息
    private void FragmentList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (FragmentList.SelectedItem is DuplicateCodeFragment fragment) {
            LocationList.ItemsSource = fragment.Locations;
            CodePreviewBox.Text = fragment.Content;
        } else {
            LocationList.ItemsSource = null;
            CodePreviewBox.Text = string.Empty;
        }
    }
}
