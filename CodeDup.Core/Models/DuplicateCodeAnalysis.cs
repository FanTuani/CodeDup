namespace CodeDup.Core.Models;

// 重复代码片段
public class DuplicateCodeFragment {
    public string Content { get; set; } = string.Empty;  // 重复的代码内容
    public int OccurrenceCount { get; set; }             // 出现次数
    public List<CodeLocation> Locations { get; set; } = new();  // 出现位置
    public int LineCount { get; set; }                   // 代码行数
    
    // 代码预览（去除前缀空格和制表符）
    public string PreviewText {
        get {
            var preview = Content.Length > 100 ? Content.Substring(0, 100) + "..." : Content;
            // 将每行前缀的空格和 tab 去掉
            var lines = preview.Split('\n');
            var trimmedLines = lines.Select(line => line.TrimStart(' ', '\t', '\r'));
            return string.Join("\n", trimmedLines).Trim();
        }
    }
}

// 代码位置
public class CodeLocation {
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string LocationText => $"{FileName} (行 {StartLine}-{EndLine})";
}

// 分析结果
public class DuplicateAnalysisResult {
    public List<DuplicateCodeFragment> Fragments { get; set; } = new();
    public int TotalFragments => Fragments.Count;
    public int TotalOccurrences => Fragments.Sum(f => f.OccurrenceCount);
}
