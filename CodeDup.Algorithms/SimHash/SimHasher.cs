using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Numerics;

namespace CodeDup.Algorithms.SimHash
{
    public static class SimHasher
    {
        public static ulong Compute64(string text)
        {
            var tokens = Tokenize(text);
            var vector = new int[64];
            foreach (var token in tokens)
            {
                var h = Hash64(token);
                for (int i = 0; i < 64; i++)
                {
                    var bit = ((h >> i) & 1UL) == 1UL ? 1 : -1;
                    vector[i] += bit;
                }
            }
            ulong result = 0UL;
            for (int i = 0; i < 64; i++)
            {
                if (vector[i] > 0) result |= (1UL << i);
            }
            return result;
        }

        public static int HammingDistance(ulong a, ulong b)
        {
            return BitOperations.PopCount(a ^ b);
        }

        private static IEnumerable<string> Tokenize(string text)
        {
            return text.ToLowerInvariant().Split(new[] { '\r', '\n', '\t', ' ', '.', ',', ';', '(', ')', '{', '}', '[', ']', '"', '\'', ':', '!', '?', '<', '>', '/', '\\', '+', '-', '*', '=' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static ulong Hash64(string token)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}


