using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace CodeDup.Text.Extractors;

public class TextExtractorDocx : ITextExtractor {
    public bool CanHandle(string extension) {
        return extension == "docx";
    }

    public string ExtractText(string filePath) {
        var text = new StringBuilder();
        try {
            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart?.Document.Body;
            if (body != null) {
                foreach (var paragraph in body.Descendants<Paragraph>()) {
                    text.AppendLine(paragraph.InnerText);
                }
            }
        } catch (Exception ex) {
            // 如果读取失败，返回错误信息
            return $"读取 DOCX 文件失败: {ex.Message}";
        }
        return text.ToString();
    }
}

public class TextExtractorPdf : ITextExtractor {
    public bool CanHandle(string extension) {
        return extension == "pdf";
    }

    public string ExtractText(string filePath) {
        try {
            var text = new StringBuilder();
            using (var document = PdfDocument.Open(filePath)) {
                foreach (var page in document.GetPages()) {
                    // 使用 GetWords() 按单词提取，保留位置信息
                    var words = page.GetWords();
                    
                    if (words.Count() == 0) {
                        continue;
                    }
                    
                    // 按照 Y 坐标分行（从上到下）
                    var lines = words.GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
                                     .OrderByDescending(g => g.Key);  // PDF 坐标系从下往上，所以倒序
                    
                    foreach (var line in lines) {
                        var lineWords = line.OrderBy(w => w.BoundingBox.Left);  // 同一行按 X 坐标排序
                        text.AppendLine(string.Join(" ", lineWords.Select(w => w.Text)));
                    }
                    
                    // 每页之间加空行
                    text.AppendLine();
                }
            }
            return text.ToString();
        }
        catch {
            // 如果 PDF 解析失败，返回空字符串
            return string.Empty;
        }
    }
}