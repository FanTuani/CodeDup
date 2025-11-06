using System.Text;
using System.Text.RegularExpressions;

namespace CodeDup.Core.Models;

public static class Preprocess {
    public static string StripCommentsAndNoise(string text, string extension) {
        var ext = extension.ToLowerInvariant();
        
        // C-style 语言：C, C++, C#, Java, JavaScript
        if (ext is "cs" or "java" or "js" or "c" or "cpp" or "h") {
            // 移除多行注释 /* ... */
            var s = Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            // 移除单行注释 //...
            s = Regex.Replace(s, @"//.*", string.Empty);
            return s;
        }
        
        // Python：使用 # 注释
        if (ext == "py") {
            // 移除单行注释 #...
            var s = Regex.Replace(text, @"#.*", string.Empty);
            // 移除多行字符串/文档注释 """...""" 或 '''...'''
            s = Regex.Replace(s, "\"\"\".*?\"\"\"", string.Empty, RegexOptions.Singleline);
            s = Regex.Replace(s, "'''.*?'''", string.Empty, RegexOptions.Singleline);
            return s;
        }
        
        // HTML/XML：移除 <!-- ... -->
        if (ext is "html" or "htm" or "xml") {
            var s = Regex.Replace(text, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);
            return s;
        }

        return text;
    }

    public static string NormalizeWhitespace(string text) {
        var sb = new StringBuilder();
        var lastSpace = false;
        foreach (var ch in text)
            if (char.IsWhiteSpace(ch)) {
                if (!lastSpace) {
                    sb.Append(' ');
                    lastSpace = true;
                }
            } else {
                sb.Append(ch);
                lastSpace = false;
            }

        return sb.ToString().Trim();
    }
}