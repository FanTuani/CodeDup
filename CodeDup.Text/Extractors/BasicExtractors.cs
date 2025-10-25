using System.Text;
using HtmlAgilityPack;
using Xceed.Words.NET;

namespace CodeDup.Text.Extractors;

public class TextExtractorTxt : ITextExtractor {
    public bool CanHandle(string extension) {
        return extension == "txt" || extension == "cs" || extension == "py" || extension == "html"
               || extension == "cpp" || extension == "c";
    }

    public string ExtractText(string filePath) {
        var ext = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();
        if (ext == "html") {
            var html = File.ReadAllText(filePath, Encoding.UTF8);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
        }

        return File.ReadAllText(filePath, Encoding.UTF8);
    }
}

public class TextExtractorDocx : ITextExtractor {
    public bool CanHandle(string extension) {
        return extension == "docx";
    }

    public string ExtractText(string filePath) {
        using var doc = DocX.Load(filePath);
        var text = new StringBuilder();
        foreach (var p in doc.Paragraphs) text.AppendLine(p.Text);
        return text.ToString();
    }
}

// PDF: TODO
public class TextExtractorPdf : ITextExtractor {
    public bool CanHandle(string extension) {
        return extension == "pdf";
    }

    public string ExtractText(string filePath) {
        // TODO
        return string.Empty;
    }
}