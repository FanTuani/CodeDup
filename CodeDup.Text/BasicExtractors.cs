using System.Text;
using Xceed.Words.NET;

namespace CodeDup.Text.Extractors;

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