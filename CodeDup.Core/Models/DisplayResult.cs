using System.Collections.Generic;

namespace CodeDup.Core.Models
{
    public class PairDisplayResult
    {
        public string FileNameA { get; set; } = string.Empty;
        public string FileNameB { get; set; } = string.Empty;
        public string FileIdA { get; set; } = string.Empty;
        public string FileIdB { get; set; } = string.Empty;
        public double Similarity { get; set; }
        public string Algorithm { get; set; } = string.Empty;
    }

    public class CenterGroupResult
    {
        public string CenterFileName { get; set; } = string.Empty;
        public string CenterFileId { get; set; } = string.Empty;
        public List<string> RelatedFileNames { get; set; } = new List<string>();
        public List<string> RelatedFileIds { get; set; } = new List<string>();
        public string RelatedFileNamesString => string.Join(", ", RelatedFileNames);
        public double MaxSimilarity { get; set; }
        public string Algorithm { get; set; } = string.Empty;
    }

    public class GroupedResult
    {
        public List<string> CleanFileNames { get; set; } = new List<string>();
        public List<string> CleanFileIds { get; set; } = new List<string>();
        public List<string> DuplicateFileNames { get; set; } = new List<string>();
        public List<string> DuplicateFileIds { get; set; } = new List<string>();
        public string Algorithm { get; set; } = string.Empty;
    }
}
