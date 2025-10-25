using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using CodeDup.App.Services;
using CodeDup.App.Views;
using CodeDup.Core.Models;
using CodeDup.Core.Storage;
using CodeDup.Text.Extractors;

namespace CodeDup.App;

public partial class MainWindow : Window
{
    private readonly IProjectStore _store;
    private readonly List<ITextExtractor> _extractors = new List<ITextExtractor>
    {
        new TextExtractorTxt(),
        new TextExtractorDocx(),
        new TextExtractorPdf()
    };

    public MainWindow()
    {
        InitializeComponent();
        _store = AppBootstrap.CreateStore();
        RefreshProjects();
        AlgoCombo.SelectedIndex = 0;
    }

    private void RefreshProjects()
    {
        ProjectList.ItemsSource = _store.ListProjects().ToList();
    }

    private void ProjectList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        RefreshFiles();
    }

    private void RefreshFiles()
    {
        if (ProjectList.SelectedItem is string project)
        {
            ApplyFilters(project);
        }
        else
        {
            FileList.ItemsSource = null;
        }
    }

    private void ApplyFilters(string project)
    {
        var files = _store.ListFiles(project).ToList();
        
        // 语言筛选
        var selectedLanguage = (LanguageFilter.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString();
        if (selectedLanguage != null && selectedLanguage != "所有语言")
        {
            files = files.Where(f => f.ProgrammingLanguage == selectedLanguage).ToList();
        }
        
        // 排序
        if (SortByDate.IsChecked == true)
        {
            files = files.OrderByDescending(f => f.ImportedAt).ToList();
        }
        else if (SortByName.IsChecked == true)
        {
            files = files.OrderBy(f => f.FileName).ToList();
        }
        else
        {
            files = files.OrderByDescending(f => f.ImportedAt).ToList();
        }
        
        FileList.ItemsSource = files;
    }

    private void FilterChanged(object sender, RoutedEventArgs e)
    {
        if (ProjectList.SelectedItem is string project)
        {
            ApplyFilters(project);
        }
    }

    private void CreateProject_Click(object sender, RoutedEventArgs e)
    {
        var name = Microsoft.VisualBasic.Interaction.InputBox("输入新项目名称", "新建项目", "Project1");
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!_store.CreateProject(name))
        {
            MessageBox.Show("项目已存在");
        }
        RefreshProjects();
    }

    private void DeleteProject_Click(object sender, RoutedEventArgs e)
    {
        if (ProjectList.SelectedItem is not string project) return;
        if (MessageBox.Show($"确认删除项目 {project}?", "确认", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            _store.DeleteProject(project);
            RefreshProjects();
            RefreshFiles();
        }
    }

    private void ImportFiles_Click(object sender, RoutedEventArgs e)
    {
        if (ProjectList.SelectedItem is not string project)
        {
            MessageBox.Show("请先选择项目");
            return;
        }
        var dlg = new OpenFileDialog
        {
            Filter = "All Supported|*.txt;*.cs;*.py;*.html;*.pdf;*.cpp;*.c",
            Multiselect = true
        };
        if (dlg.ShowDialog() == true)
        {
            ImportPaths(project, dlg.FileNames);
        }
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (ProjectList.SelectedItem is not string project) return;
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            ImportPaths(project, paths);
        }
    }

    private void FileList_Drop(object sender, DragEventArgs e)
    {
        Window_Drop(sender, e);
    }

    private void ImportPaths(string project, IEnumerable<string> paths)
    {
        foreach (var p in paths)
        {
            var ext = System.IO.Path.GetExtension(p).TrimStart('.').ToLowerInvariant();
            bool overwrite = false;
            var meta = _store.ListFiles(project).FirstOrDefault(f => f.FileName.Equals(System.IO.Path.GetFileName(p), System.StringComparison.OrdinalIgnoreCase));
            if (meta != null)
            {
                var r = MessageBox.Show($"检测到同名文件 {meta.FileName}，是否替换?", "同名文件", MessageBoxButton.YesNoCancel);
                if (r == MessageBoxResult.Cancel) break;
                overwrite = r == MessageBoxResult.Yes;
            }

            var handled = _extractors.FirstOrDefault(x => x.CanHandle(ext));
            bool skipped;
            var added = _store.AddFile(project, p, overwrite, out skipped);
            if (skipped) continue;
            if (handled != null)
            {
                try
                {
                    var text = handled.ExtractText(p);
                    var cleaned = Preprocess.NormalizeWhitespace(Preprocess.StripCommentsAndNoise(text, ext));
                    var dest = _store.GetFileContentPath(project, added.Id);
                    System.IO.File.WriteAllText(dest, cleaned);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"处理文件 {System.IO.Path.GetFileName(p)} 时出错: {ex.Message}", "处理错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        RefreshFiles();
    }

    private void RunCompare_Click(object sender, RoutedEventArgs e)
    {
        if (ProjectList.SelectedItem is not string project)
        {
            MessageBox.Show("请先选择项目");
            return;
        }
        if (!double.TryParse(ThresholdBox.Text, out var threshold)) threshold = 0.8;
        var algo = (AlgoCombo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Winnowing";

        var files = _store.ListFiles(project).ToList();
        var pairs = new List<PairSimilarity>();
        for (int i = 0; i < files.Count; i++)
        {
            for (int j = i + 1; j < files.Count; j++)
            {
                var aPath = _store.GetFileContentPath(project, files[i].Id);
                var bPath = _store.GetFileContentPath(project, files[j].Id);
                var aText = System.IO.File.Exists(aPath) ? System.IO.File.ReadAllText(aPath) : string.Empty;
                var bText = System.IO.File.Exists(bPath) ? System.IO.File.ReadAllText(bPath) : string.Empty;
                double sim = 0.0;
                switch (algo)
                {
                    case "Winnowing":
                        var fa = CodeDup.Algorithms.Winnowing.WinnowingFingerprint.Compute(aText);
                        var fb = CodeDup.Algorithms.Winnowing.WinnowingFingerprint.Compute(bText);
                        var sa = fa.Select(f => f.hash).ToHashSet();
                        var sb = fb.Select(f => f.hash).ToHashSet();
                        var inter = sa.Intersect(sb).Count();
                        var union = sa.Union(sb).Count();
                        sim = union == 0 ? 0.0 : (double)inter / union;
                        break;
                    case "SimHash":
                        var ha = CodeDup.Algorithms.SimHash.SimHasher.Compute64(aText);
                        var hb = CodeDup.Algorithms.SimHash.SimHasher.Compute64(bText);
                        var dist = CodeDup.Algorithms.SimHash.SimHasher.HammingDistance(ha, hb);
                        sim = 1.0 - dist / 64.0;
                        break;
                    default:
                        sim = CodeDup.Algorithms.Shingle.ShingleCosine.Similarity(aText, bText);
                        break;
                }
                if (sim >= threshold)
                {
                    pairs.Add(new PairSimilarity
                    {
                        FileIdA = files[i].Id,
                        FileIdB = files[j].Id,
                        Similarity = sim,
                        Algorithm = algo
                    });
                }
            }
        }
        // 生成三种显示结果
        var pairResults = GeneratePairResults(pairs, files);
        var centerResults = GenerateCenterResults(pairs, files);
        var groupedResults = GenerateGroupedResults(pairs, files);
        
        // 显示结果
        DisplayResults(pairResults, centerResults, groupedResults);
    }

    private List<PairDisplayResult> GeneratePairResults(List<PairSimilarity> pairs, List<CodeFileMetadata> files)
    {
        var fileDict = files.ToDictionary(f => f.Id, f => f.FileName);
        return pairs.Select(p => new PairDisplayResult
        {
            FileNameA = fileDict.GetValueOrDefault(p.FileIdA, p.FileIdA),
            FileNameB = fileDict.GetValueOrDefault(p.FileIdB, p.FileIdB),
            FileIdA = p.FileIdA,
            FileIdB = p.FileIdB,
            Similarity = p.Similarity,
            Algorithm = p.Algorithm
        }).OrderByDescending(p => p.Similarity).ToList();
    }

    private List<CenterGroupResult> GenerateCenterResults(List<PairSimilarity> pairs, List<CodeFileMetadata> files)
    {
        var fileDict = files.ToDictionary(f => f.Id, f => f.FileName);
        var centerGroups = new List<CenterGroupResult>();
        var processedFiles = new HashSet<string>();

        foreach (var file in files)
        {
            if (processedFiles.Contains(file.Id)) continue;

            var relatedPairs = pairs.Where(p => p.FileIdA == file.Id || p.FileIdB == file.Id).ToList();
            if (relatedPairs.Count == 0) continue;

            var relatedFiles = new HashSet<string> { file.Id };
            var maxSim = 0.0;

            foreach (var pair in relatedPairs)
            {
                var otherFileId = pair.FileIdA == file.Id ? pair.FileIdB : pair.FileIdA;
                relatedFiles.Add(otherFileId);
                maxSim = Math.Max(maxSim, pair.Similarity);
            }

            centerGroups.Add(new CenterGroupResult
            {
                CenterFileName = file.FileName,
                CenterFileId = file.Id,
                RelatedFileNames = relatedFiles.Where(f => f != file.Id).Select(f => fileDict.GetValueOrDefault(f, f)).ToList(),
                RelatedFileIds = relatedFiles.Where(f => f != file.Id).ToList(),
                MaxSimilarity = maxSim,
                Algorithm = relatedPairs.FirstOrDefault()?.Algorithm ?? ""
            });

            foreach (var f in relatedFiles)
                processedFiles.Add(f);
        }

        return centerGroups.OrderByDescending(g => g.MaxSimilarity).ToList();
    }

    private GroupedResult GenerateGroupedResults(List<PairSimilarity> pairs, List<CodeFileMetadata> files)
    {
        var fileDict = files.ToDictionary(f => f.Id, f => f.FileName);
        var duplicateFiles = new HashSet<string>();

        foreach (var pair in pairs)
        {
            duplicateFiles.Add(pair.FileIdA);
            duplicateFiles.Add(pair.FileIdB);
        }

        var cleanFiles = files.Where(f => !duplicateFiles.Contains(f.Id)).ToList();
        var duplicateFileList = files.Where(f => duplicateFiles.Contains(f.Id)).ToList();

        return new GroupedResult
        {
            CleanFileNames = cleanFiles.Select(f => f.FileName).ToList(),
            CleanFileIds = cleanFiles.Select(f => f.Id).ToList(),
            DuplicateFileNames = duplicateFileList.Select(f => f.FileName).ToList(),
            DuplicateFileIds = duplicateFileList.Select(f => f.Id).ToList(),
            Algorithm = pairs.FirstOrDefault()?.Algorithm ?? ""
        };
    }

    private void DisplayResults(List<PairDisplayResult> pairResults, List<CenterGroupResult> centerResults, GroupedResult groupedResult)
    {
        EnsureResultViews();
        
        // 显示两两分组结果
        var pairList = (System.Windows.Controls.ListView)((System.Windows.Controls.TabItem)ResultTabs.Items[0]).Content;
        pairList.ItemsSource = pairResults;

        // 显示中心分组结果
        var centerList = (System.Windows.Controls.ListView)((System.Windows.Controls.TabItem)ResultTabs.Items[1]).Content;
        centerList.ItemsSource = centerResults;

        // 显示分组结果
        var groupedList = (System.Windows.Controls.ListView)((System.Windows.Controls.TabItem)ResultTabs.Items[2]).Content;
        var groupedDisplay = new List<object>();
        groupedDisplay.Add(new { Category = "无重复文件", Files = string.Join(", ", groupedResult.CleanFileNames) });
        groupedDisplay.Add(new { Category = "有重复文件", Files = string.Join(", ", groupedResult.DuplicateFileNames) });
        groupedList.ItemsSource = groupedDisplay;
    }

    private void EnsureResultViews()
    {
        // 两两分组视图
        if (((System.Windows.Controls.TabItem)ResultTabs.Items[0]).Content == null)
        {
            var lv = new System.Windows.Controls.ListView();
            lv.MouseDoubleClick += (s, e) => {
                if (lv.SelectedItem != null)
                {
                    CompareFiles_Click(s, e);
                }
            };
            var gv = new System.Windows.Controls.GridView();
            gv.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = "文件A", DisplayMemberBinding = new System.Windows.Data.Binding("FileNameA"), Width = 200 });
            gv.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = "文件B", DisplayMemberBinding = new System.Windows.Data.Binding("FileNameB"), Width = 200 });
            gv.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = "相似度", DisplayMemberBinding = new System.Windows.Data.Binding("Similarity"), Width = 100 });
            lv.View = gv;
            ((System.Windows.Controls.TabItem)ResultTabs.Items[0]).Content = lv;
        }

        // 中心分组视图
        if (((System.Windows.Controls.TabItem)ResultTabs.Items[1]).Content == null)
        {
            var lv = new System.Windows.Controls.ListView();
            var gv = new System.Windows.Controls.GridView();
            gv.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = "中心文件", DisplayMemberBinding = new System.Windows.Data.Binding("CenterFileName"), Width = 200 });
            gv.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = "相关文件", DisplayMemberBinding = new System.Windows.Data.Binding("RelatedFileNamesString"), Width = 300 });
            gv.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = "最高相似度", DisplayMemberBinding = new System.Windows.Data.Binding("MaxSimilarity"), Width = 100 });
            lv.View = gv;
            ((System.Windows.Controls.TabItem)ResultTabs.Items[1]).Content = lv;
        }

        // 分组显示视图
        if (((System.Windows.Controls.TabItem)ResultTabs.Items[2]).Content == null)
        {
            var lv = new System.Windows.Controls.ListView();
            var gv = new System.Windows.Controls.GridView();
            gv.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = "分类", DisplayMemberBinding = new System.Windows.Data.Binding("Category"), Width = 150 });
            gv.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = "文件列表", DisplayMemberBinding = new System.Windows.Data.Binding("Files"), Width = 500 });
            lv.View = gv;
            ((System.Windows.Controls.TabItem)ResultTabs.Items[2]).Content = lv;
        }
    }

    private void ExportResults_Click(object sender, RoutedEventArgs e)
    {
        if (ProjectList.SelectedItem is not string project)
        {
            MessageBox.Show("请先选择项目");
            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Filter = "CSV文件|*.csv|文本文件|*.txt",
            DefaultExt = "csv",
            FileName = $"查重结果_{project}_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                var files = _store.ListFiles(project).ToList();
                var content = new System.Text.StringBuilder();
                
                // 添加文件信息
                content.AppendLine("文件列表:");
                content.AppendLine("文件名,语言,大小(字节),导入时间");
                foreach (var file in files)
                {
                    content.AppendLine($"{file.FileName},{file.ProgrammingLanguage},{file.FileSizeBytes},{file.ImportedAt:yyyy-MM-dd HH:mm:ss}");
                }
                
                // 添加查重结果（如果有的话）
                var pairList = (System.Windows.Controls.ListView)((System.Windows.Controls.TabItem)ResultTabs.Items[0]).Content;
                if (pairList?.ItemsSource is IEnumerable<PairDisplayResult> pairs)
                {
                    content.AppendLine("\n查重结果:");
                    content.AppendLine("文件A,文件B,相似度,算法");
                    foreach (var pair in pairs)
                    {
                        content.AppendLine($"{pair.FileNameA},{pair.FileNameB},{pair.Similarity:F4},{pair.Algorithm}");
                    }
                }
                
                System.IO.File.WriteAllText(saveDialog.FileName, content.ToString(), System.Text.Encoding.UTF8);
                MessageBox.Show($"结果已导出到: {saveDialog.FileName}", "导出成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void CompareFiles_Click(object sender, RoutedEventArgs e)
    {
        if (ProjectList.SelectedItem is not string project)
        {
            MessageBox.Show("请先选择项目");
            return;
        }

        // 获取选中的文件对
        var pairList = (System.Windows.Controls.ListView)((System.Windows.Controls.TabItem)ResultTabs.Items[0]).Content;
        if (pairList?.SelectedItem is not PairDisplayResult selectedPair)
        {
            MessageBox.Show("请先在两两分组中选择要对比的文件对");
            return;
        }

        // 打开双文件对比窗口
        var compareWindow = new FileCompareWindow(project, selectedPair, _store);
        compareWindow.Owner = this;
        compareWindow.ShowDialog();
    }
}