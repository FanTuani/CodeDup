using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Xceed.Words.NET;

namespace CodeDup.Text.Extractors
{
    public class TextExtractorTxt : ITextExtractor
    {
        public bool CanHandle(string extension) => extension == "txt" || extension == "cs" || extension == "py" || extension == "html";

        public string ExtractText(string filePath)
        {
            var ext = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();
            if (ext == "html")
            {
                var html = File.ReadAllText(filePath, Encoding.UTF8);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
            }
            return File.ReadAllText(filePath, Encoding.UTF8);
        }
    }

    public class TextExtractorDocx : ITextExtractor
    {
        public bool CanHandle(string extension) => extension == "docx";

        public string ExtractText(string filePath)
        {
            using var doc = DocX.Load(filePath);
            var text = new StringBuilder();
            foreach (var p in doc.Paragraphs)
            {
                text.AppendLine(p.Text);
            }
            return text.ToString();
        }
    }

    // PDF: 简化处理，课程设计不需要复杂的PDF处理
    public class TextExtractorPdf : ITextExtractor
    {
        public bool CanHandle(string extension) => extension == "pdf";

        public string ExtractText(string filePath)
        {
            // 课程设计简化处理：PDF文件直接跳过
            return string.Empty;
        }
    }
}


