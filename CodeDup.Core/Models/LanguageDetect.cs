using System.IO;

namespace CodeDup.Core.Models
{
    public static class LanguageDetect
    {
        public static string FromExtension(string extension)
        {
            var ext = extension.ToLowerInvariant();
            return ext switch
            {
                "cs" => "C#",
                "py" => "Python",
                "html" => "HTML",
                "txt" => "Text",
                "docx" => "DOCX",
                "pdf" => "PDF",
                _ => ext
            };
        }

        public static string FromPath(string path)
        {
            return FromExtension(Path.GetExtension(path).TrimStart('.'));
        }
    }
}


