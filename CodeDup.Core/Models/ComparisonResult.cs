namespace CodeDup.Core.Models;

public class PairSimilarity {
    public string FileIdA { get; set; } = string.Empty;
    public string FileIdB { get; set; } = string.Empty;
    public double Similarity { get; set; }
    public string Algorithm { get; set; } = string.Empty;
}

public class ComparisonReport {
    public string ProjectName { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public List<PairSimilarity> Pairs { get; set; } = new();
}