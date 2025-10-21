using System.Linq;
using System.Text;

namespace CodeDup.Core.Models
{
    public static class Preprocess
    {
        public static string StripCommentsAndNoise(string text, string extension)
        {
            var ext = extension.ToLowerInvariant();
            if (ext is "cs" or "java" or "js" or "c" or "cpp" or "h" or "py")
            {
                // 简化规则：移除 //... 和 /* ... */ 以及 Python # ...
                var s = System.Text.RegularExpressions.Regex.Replace(text, @"/\*.*?\*/", string.Empty, System.Text.RegularExpressions.RegexOptions.Singleline);
                s = System.Text.RegularExpressions.Regex.Replace(s, @"//.*", string.Empty);
                s = System.Text.RegularExpressions.Regex.Replace(s, @"#.*", string.Empty);
                return s;
            }
            return text;
        }

        public static string NormalizeWhitespace(string text)
        {
            var sb = new StringBuilder();
            bool lastSpace = false;
            foreach (var ch in text)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!lastSpace)
                    {
                        sb.Append(' ');
                        lastSpace = true;
                    }
                }
                else
                {
                    sb.Append(ch);
                    lastSpace = false;
                }
            }
            return sb.ToString().Trim();
        }
    }
}


