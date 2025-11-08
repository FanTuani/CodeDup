using System.Text;
using CodeDup.Core.Models;
using CodeDup.Core.Storage;

namespace CodeDup.Core.Services;

// 重复代码分析服务
public class DuplicateCodeAnalyzer {
    private readonly IProjectStore _store;

    public DuplicateCodeAnalyzer(IProjectStore store) {
        _store = store;
    }

    // 分析重复代码片段
    public DuplicateAnalysisResult AnalyzeDuplicateCode(
        string project,
        List<string> fileIds,
        List<CodeFileMetadata> allFiles,
        int minOccurrences,
        int minLineCount = 3) {
        
        var result = new DuplicateAnalysisResult();
        
        // 1. 读取所有文件内容（预处理后）
        var fileContents = new Dictionary<string, List<string>>();
        var fileDict = allFiles.ToDictionary(f => f.Id);
        
        foreach (var fileId in fileIds) {
            var filePath = _store.GetFileContentPath(project, fileId);
            if (File.Exists(filePath)) {
                try {
                    var content = File.ReadAllText(filePath, Encoding.UTF8);
                    var extension = fileDict[fileId].Extension;
                    
                    // 预处理：移除注释和噪音
                    content = Preprocess.StripCommentsAndNoise(content, extension);
                    
                    // 分行，过滤空行和琐碎行（HTML 声明、简单标签、括号等）
                    var lines = content.Split('\n')
                        .Select(l => l.TrimEnd('\r'))
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .Where(l => !Preprocess.IsTrivialLine(l, extension))  // 过滤琐碎行
                        .ToList();
                    
                    fileContents[fileId] = lines;
                }
                catch {
                    // 忽略读取失败的文件
                }
            }
        }
        
        // 2. 使用滑动窗口提取代码片段（连续 N 行为一个片段）
        var fragmentMap = new Dictionary<string, DuplicateCodeFragment>();
        
        foreach (var (fileId, lines) in fileContents) {
            for (int i = 0; i <= lines.Count - minLineCount; i++) {
                // 提取连续 minLineCount 行
                var fragmentLines = lines.Skip(i).Take(minLineCount).ToList();
                var fragmentContent = string.Join("\n", fragmentLines);
                
                // 忽略太短或全是空白的片段
                if (fragmentContent.Trim().Length < 20) {
                    continue;
                }
                
                // 计算片段的哈希键（用于去重）
                var fragmentKey = fragmentContent.Trim();
                
                if (!fragmentMap.ContainsKey(fragmentKey)) {
                    fragmentMap[fragmentKey] = new DuplicateCodeFragment {
                        Content = fragmentContent,
                        LineCount = minLineCount,
                        Locations = new List<CodeLocation>()
                    };
                }
                
                // 添加位置信息
                fragmentMap[fragmentKey].Locations.Add(new CodeLocation {
                    FileId = fileId,
                    FileName = fileDict[fileId].FileName,
                    StartLine = i + 1,
                    EndLine = i + minLineCount
                });
            }
        }
        
        // 3. 统计出现次数（跨文件去重）
        foreach (var fragment in fragmentMap.Values) {
            // 按文件去重：同一个文件中的多次出现只算一次
            var uniqueFiles = fragment.Locations
                .GroupBy(loc => loc.FileId)
                .Select(g => g.First())  // 每个文件只取第一次出现
                .ToList();
            
            fragment.OccurrenceCount = uniqueFiles.Count;
            fragment.Locations = uniqueFiles;  // 只保留每个文件的第一次出现
        }
        
        // 4. 过滤并排序
        result.Fragments = fragmentMap.Values
            .Where(f => f.OccurrenceCount >= minOccurrences)
            .OrderByDescending(f => f.OccurrenceCount)
            .ThenByDescending(f => f.LineCount)
            .ToList();
        
        return result;
    }
}
