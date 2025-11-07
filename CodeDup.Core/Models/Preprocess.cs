using System.Linq;
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

    /// <summary>
    /// 移除HTML的结构性标签，只保留实际内容
    /// 用于查重算法，将HTML简化为纯文本内容
    /// </summary>
    public static string StripHtmlStructure(string html) {
        try {
            // 需要 HtmlAgilityPack，但这是 Core 项目，不应该依赖外部库
            // 使用简单的正则替换来移除HTML标签
            // 移除 script 和 style 标签及其内容
            var text = Regex.Replace(html, @"<script[^>]*>.*?</script>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<style[^>]*>.*?</style>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            
            // 移除所有其他HTML标签
            text = Regex.Replace(text, @"<[^>]+>", " ");
            
            // 解码HTML实体
            text = System.Net.WebUtility.HtmlDecode(text);
            
            return text;
        }
        catch {
            return html;
        }
    }

    /// <summary>
    /// 判断一行代码是否是"琐碎"的（结构性的、无实际内容的）
    /// 这些行即使重复也不应该被高亮显示
    /// </summary>
    public static bool IsTrivialLine(string text, string? extension = null) {
        // 去除前后空白
        var trimmed = text.Trim();
        
        // 空行
        if (string.IsNullOrEmpty(trimmed)) {
            return true;
        }

        // 只包含单个字符的行（通常是括号）
        if (trimmed.Length == 1) {
            return true;
        }

        // HTML/XML 相关的琐碎行
        var ext = extension?.ToLowerInvariant();
        if (ext is "html" or "htm" or "xml") {
            // 检查是否是常见的HTML声明或模板标签
            if (IsCommonHtmlDeclaration(trimmed)) {
                return true;
            }

            // 检查是否是简单的HTML标签（不带属性）
            if (IsSimpleHtmlTag(trimmed)) {
                return true;
            }
        }

        // 只包含括号、花括号、方括号的组合
        var cleaned = trimmed.Replace("{", "")
                            .Replace("}", "")
                            .Replace("(", "")
                            .Replace(")", "")
                            .Replace("[", "")
                            .Replace("]", "")
                            .Replace("<", "")
                            .Replace(">", "")
                            .Replace(";", "")
                            .Replace(",", "")
                            .Trim();
        
        // 如果移除这些符号后为空，说明这行只包含这些符号
        return string.IsNullOrEmpty(cleaned);
    }

    private static bool IsCommonHtmlDeclaration(string text) {
        // 常见的HTML声明和模板标签（大小写不敏感）
        var lower = text.ToLower();
        
        // DOCTYPE声明
        if (lower.StartsWith("<!doctype")) {
            return true;
        }

        // 常见的html标签变体（带lang属性）
        if (lower == "<html>" || lower == "</html>" || 
            lower.StartsWith("<html lang=") || lower.StartsWith("<html xmlns=")) {
            return true;
        }

        // meta charset标签
        if (lower.StartsWith("<meta charset=")) {
            return true;
        }

        // 其他常见的meta标签
        if (lower == "<meta>" || lower == "</meta>") {
            return true;
        }

        return false;
    }

    private static bool IsSimpleHtmlTag(string text) {
        // 检查是否匹配简单的HTML标签模式（开标签或闭标签，不含属性）
        // 例如: <div>, </div>, <script>, </script>, <body>, </body>
        // 但不匹配: <div id="xxx">, <script src="xxx">
        
        if (!text.StartsWith("<") || !text.EndsWith(">")) {
            return false;
        }

        // 移除尖括号
        var inner = text.Substring(1, text.Length - 2).Trim();
        
        // 检查闭合标签 (如 </div>)
        if (inner.StartsWith("/")) {
            inner = inner.Substring(1).Trim();
        }

        // 如果包含空格，说明有属性，不是简单标签
        if (inner.Contains(" ")) {
            return false;
        }

        // 如果包含等号，说明有属性，不是简单标签
        if (inner.Contains("=")) {
            return false;
        }

        // 检查是否是有效的标签名（只包含字母、数字、短横线）
        // 如果标签名合法且不含属性，则认为是简单标签
        return inner.Length > 0 && inner.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
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