using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
    private readonly AiAnalysisService _aiService;

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
        _aiService = new AiAnalysisService();
    }

    // RichTextBox 辅助方法 - 设置文本
    private void SetRichText(string text) {
        AiReportBox.Document.Blocks.Clear();
        AiReportBox.Document.Blocks.Add(new Paragraph(new Run(text)));
    }

    // RichTextBox 辅助方法 - 追加文本（高性能）
    private void AppendRichText(string text) {
        var paragraph = AiReportBox.Document.Blocks.LastBlock as Paragraph;
        if (paragraph == null) {
            paragraph = new Paragraph();
            AiReportBox.Document.Blocks.Add(paragraph);
        }
        paragraph.Inlines.Add(new Run(text));
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
            SetRichText("等待重复代码分析完成...");
            
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
                SetRichText("未找到符合条件的重复代码。");
                MessageBox.Show("未找到符合条件的重复代码", "分析完成", MessageBoxButton.OK, MessageBoxImage.Information);
            } else {
                SetRichText("点击上方按钮生成 AI 分析报告...");
            }
        }
        catch (Exception ex) {
            MessageBox.Show($"分析失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // 生成 AI 分析报告按钮点击事件（使用流式传输 + 批量更新优化）
    private async void GenerateReport_Click(object sender, RoutedEventArgs e) {
        if (_analysisResult == null || _analysisResult.TotalFragments == 0) {
            MessageBox.Show("请先进行重复代码分析", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try {
            // 禁用按钮，显示进度
            GenerateReportButton.IsEnabled = false;
            GenerateReportButton.Content = "AI 分析中，请稍候...";
            SetRichText("AI 分析报告：\n\n");

            var buffer = new StringBuilder();
            var lastUpdateTime = DateTime.Now;
            const int UPDATE_INTERVAL_MS = 1; // 每 1ms 更新一次 UI

            // 使用流式传输，批量更新 UI 以提高性能
            await foreach (var chunk in _aiService.AnalyzeDuplicateCodeStream(_analysisResult, topN: 10)) {
                buffer.Append(chunk);
                
                // 每隔一定时间或累积足够内容后更新 UI
                var elapsed = (DateTime.Now - lastUpdateTime).TotalMilliseconds;
                if (elapsed >= UPDATE_INTERVAL_MS || buffer.Length >= 50) {
                    AppendRichText(buffer.ToString());
                    AiReportBox.ScrollToEnd();
                    buffer.Clear();
                    lastUpdateTime = DateTime.Now;
                    
                    // 让 UI 有机会响应
                    await Task.Delay(1);
                }
            }
            
            // 刷新剩余内容
            if (buffer.Length > 0) {
                AppendRichText(buffer.ToString());
                AiReportBox.ScrollToEnd();
            }
        }
        catch (Exception ex) {
            AppendRichText($"\n\n生成报告失败：{ex.Message}");
            MessageBox.Show($"生成报告失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally {
            // 恢复按钮状态
            GenerateReportButton.IsEnabled = true;
            GenerateReportButton.Content = "生成 AI 分析报告（前 10 个片段）";
        }
    }
}
