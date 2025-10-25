namespace CodeDup.Algorithms.Winnowing;

public static class WinnowingFingerprint {
    public static IReadOnlyList<(int hash, int pos)> Compute(string text, int k = 5, int window = 4) {
        var normalized = Normalize(text);
        var hashes = new List<(int hash, int pos)>();
        var rolling = new RollingHash(k);
        for (var i = 0; i < normalized.Length; i++) {
            rolling.Push(normalized[i]);
            if (i + 1 >= k) hashes.Add((rolling.Value, i - k + 1));
        }

        // window min selection
        var result = new List<(int hash, int pos)>();
        int? lastPos = null;
        for (var i = 0; i + window - 1 < hashes.Count; i++) {
            var slice = hashes.GetRange(i, window);
            var min = slice.Select((h, idx) => (h.hash, h.pos, idx)).OrderBy(t => t.hash).ThenBy(t => t.idx).First();
            if (lastPos != min.pos) {
                result.Add((min.hash, min.pos));
                lastPos = min.pos;
            }
        }

        return result;
    }

    private static string Normalize(string text) {
        var chars = text.ToLowerInvariant().Where(char.IsLetterOrDigit);
        return new string(chars.ToArray());
    }

    private sealed class RollingHash {
        private const int Base = 257;
        private const int Mod = 1_000_000_007;
        private readonly int _k;
        private readonly Queue<char> _queue = new();
        private readonly int _power;

        public RollingHash(int k) {
            _k = k;
            _power = 1;
            for (var i = 0; i < k - 1; i++) _power = (int)((long)_power * Base % Mod);
        }

        public int Value { get; private set; }

        public void Push(char c) {
            if (_queue.Count == _k) {
                var oldest = _queue.Dequeue();
                Value = (int)((Value - (long)oldest * _power % Mod + Mod) % Mod);
            }

            _queue.Enqueue(c);
            Value = (int)(((long)Value * Base + c) % Mod);
        }
    }
}