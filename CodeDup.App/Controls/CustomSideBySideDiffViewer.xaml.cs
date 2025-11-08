using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CodeDup.Core.Models;
using DiffPlex.DiffBuilder.Model;

namespace CodeDup.App.Controls;

public partial class CustomSideBySideDiffViewer : UserControl {
    private bool _isScrolling = false;
    private string? _extensionA;
    private string? _extensionB;

    public CustomSideBySideDiffViewer() {
        InitializeComponent();
    }

    public void SetDiffModel(SideBySideDiffModel model, string? extensionA = null, string? extensionB = null) {
        _extensionA = extensionA;
        _extensionB = extensionB;
        
        LeftItemsControl.Items.Clear();
        RightItemsControl.Items.Clear();

        if (model == null) return;

        // 渲染左侧
        foreach (var line in model.OldText.Lines) {
            var textBlock = CreateTextBlock(line, _extensionA);
            LeftItemsControl.Items.Add(new Border {
                BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = textBlock
            });
        }

        // 渲染右侧
        foreach (var line in model.NewText.Lines) {
            var textBlock = CreateTextBlock(line, _extensionB);
            RightItemsControl.Items.Add(new Border {
                BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = textBlock
            });
        }
    }

    private TextBlock CreateTextBlock(DiffPiece line, string? extension) {
        var textBlock = new TextBlock {
            TextWrapping = TextWrapping.NoWrap,
            Padding = new Thickness(5, 2, 5, 2),
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12
        };

        // 将 Tab 转换为 4 个空格
        var processedText = line.Text?.Replace("\t", "    ");

        // 根据行的类型设置样式
        switch (line.Type) {
            case ChangeType.Unchanged:
                // 重复的行 - 使用黄色背景高亮文字部分
                AddHighlightedText(textBlock, processedText, extension);
                break;
            case ChangeType.Deleted:
            case ChangeType.Inserted:
                // 不同的行 - 灰色文字，无背景
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                textBlock.Text = processedText ?? string.Empty;
                break;
            case ChangeType.Imaginary:
                // 占位行
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204));
                textBlock.Text = processedText ?? string.Empty;
                break;
            case ChangeType.Modified:
                // 修改的行 - 灰色文字
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                textBlock.Text = processedText ?? string.Empty;
                break;
        }

        return textBlock;
    }

    private void AddHighlightedText(TextBlock textBlock, string? text, string? extension) {
        if (string.IsNullOrEmpty(text)) {
            return;
        }

        // 使用统一的 Preprocess.IsTrivialLine 判断是否是"无意义"的重复行
        if (Preprocess.IsTrivialLine(text, extension)) {
            // 不高亮，显示为普通灰色文字
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            return;
        }

        // 对于重复的行，使用Run来只高亮有字符的部分
        var run = new Run(text) {
            Background = new SolidColorBrush(Color.FromArgb(255, 255, 235, 59)), // 黄色背景
            Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)) // 黑色文字
        };
        textBlock.Inlines.Add(run);
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
        if (_isScrolling) return;

        _isScrolling = true;

        try {
            var source = sender as ScrollViewer;
            if (source == LeftScrollViewer) {
                RightScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
                RightScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            } else if (source == RightScrollViewer) {
                LeftScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
                LeftScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        } finally {
            _isScrolling = false;
        }
    }
}
