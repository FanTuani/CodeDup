using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Xceed.Words.NET;

namespace CodeDup.Text.Extractors
{
    public class TextExtractorTxt : ITextExtractor
    {
        public bool CanHandle(string extension) => extension == "txt" || extension == "cs" || extension == "py" || extension == "html";

        public async Task<string> ExtractTextAsync(string filePath)
        {
            var ext = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();
            if (ext == "html")
            {
                var html = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
            }
            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }
    }

    public class TextExtractorDocx : ITextExtractor
    {
        public bool CanHandle(string extension) => extension == "docx";

        public Task<string> ExtractTextAsync(string filePath)
        {
            using var doc = DocX.Load(filePath);
            var text = new StringBuilder();
            foreach (var p in doc.Paragraphs)
            {
                text.AppendLine(p.Text);
            }
            return Task.FromResult(text.ToString());
        }
    }

    // PDF: 使用 Xceed.Pdf 从 DocX 套件，只提取简单文本（适配器可替换为 PdfPig）
    public class TextExtractorPdf : ITextExtractor
    {
        public bool CanHandle(string extension) => extension == "pdf";

        public Task<string> ExtractTextAsync(string filePath)
        {
            // 简化处理：某些 PDF 可能无法完整提取，实际项目可替换为 PdfPig 或 Tesseract OCR
            // 这里返回空字符串代表不可提取，将在预处理阶段被忽略
            return Task.FromResult(string.Empty);
        }
    }
}


