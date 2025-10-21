using System;
using System.IO;
using System.Windows;
using CodeDup.Core.Models;
using CodeDup.Core.Storage;

namespace CodeDup.App.Views
{
    public partial class FileCompareWindow : Window
    {
        private readonly string _project;
        private readonly PairDisplayResult _pair;
        private readonly IProjectStore _store;

        public FileCompareWindow(string project, PairDisplayResult pair, IProjectStore store)
        {
            InitializeComponent();
            _project = project;
            _pair = pair;
            _store = store;
            
            LoadFileContents();
        }

        private void LoadFileContents()
        {
            try
            {
                // 设置标签
                FileALabel.Text = _pair.FileNameA;
                FileBLabel.Text = _pair.FileNameB;
                SimilarityLabel.Text = $"{_pair.Similarity:P2}";

                // 加载文件内容
                var fileAPath = _store.GetFileContentPath(_project, _pair.FileIdA);
                var fileBPath = _store.GetFileContentPath(_project, _pair.FileIdB);

                var contentA = File.Exists(fileAPath) ? File.ReadAllText(fileAPath) : "文件内容无法读取";
                var contentB = File.Exists(fileBPath) ? File.ReadAllText(fileBPath) : "文件内容无法读取";

                FileAText.Text = contentA;
                FileBText.Text = contentB;

                // 简单的差异高亮（这里只是基础实现，实际项目中可以使用更高级的diff算法）
                HighlightDifferences();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载文件内容时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HighlightDifferences()
        {
            // 简单的差异高亮实现
            // 在实际项目中，这里可以使用更高级的diff算法来高亮显示具体的差异行
            
            var linesA = FileAText.Text.Split('\n');
            var linesB = FileBText.Text.Split('\n');
            
            // 这里可以添加更复杂的差异检测和高亮逻辑
            // 目前只是显示两个文件的内容，用户可以手动比较
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
