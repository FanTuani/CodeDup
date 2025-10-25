namespace CodeDup.Text.Extractors;

public interface ITextExtractor {
    bool CanHandle(string extension);
    string ExtractText(string filePath);
}