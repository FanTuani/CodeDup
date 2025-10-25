namespace CodeDup.Core.Models;

public class CodeFileMetadata {
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty; // relative to project folder
    public string ProjectName { get; set; } = string.Empty;
    public string ProgrammingLanguage { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}