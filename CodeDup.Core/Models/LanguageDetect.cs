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
                "html" or "htm" => "HTML",
                "txt" => "Text",
                "js" => "JavaScript",
                "java" => "Java",
                "cpp" or "cxx" => "C++",
                "c" => "C",
                "xml" => "XML",
                "json" => "JSON",
                "css" => "CSS",
                "sql" => "SQL",
                "docx" => "DOCX",
                "pdf" => "PDF",
                _ => ext.ToUpperInvariant()
            };
        }

        public static string FromPath(string path)
        {
            return FromExtension(Path.GetExtension(path).TrimStart('.'));
        }
    }
}


