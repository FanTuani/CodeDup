using System.Text;

namespace CodeDup.Algorithms.Shingle;

public static class ShingleCosine {
    public static double Similarity(string a, string b, int k = 5) {
        var va = BuildVector(a, k);
        var vb = BuildVector(b, k);
        if (va.Count == 0 || vb.Count == 0) return 0.0;
        var dot = va.Sum(kv => vb.TryGetValue(kv.Key, out var w) ? kv.Value * w : 0);
        var na = Math.Sqrt(va.Sum(kv => kv.Value * kv.Value));
        var nb = Math.Sqrt(vb.Sum(kv => kv.Value * kv.Value));
        if (na == 0 || nb == 0) return 0.0;
        return dot / (na * nb);
    }

    private static Dictionary<string, int> BuildVector(string s, int k) {
        var norm = Normalize(s);
        var dict = new Dictionary<string, int>();
        if (norm.Length < k) return dict;
        for (var i = 0; i <= norm.Length - k; i++) {
            var sh = norm.Substring(i, k);
            dict.TryGetValue(sh, out var c);
            dict[sh] = c + 1;
        }

        return dict;
    }

    private static string Normalize(string text) {
        var sb = new StringBuilder();
        foreach (var ch in text)
            if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
            else if (char.IsWhiteSpace(ch)) sb.Append(' ');
        return sb.ToString();
    }
}