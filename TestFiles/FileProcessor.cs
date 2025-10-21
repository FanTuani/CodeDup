using System;
using System.IO;
using System.Text;

namespace FileUtils
{
    public class FileProcessor
    {
        public static string ReadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty");
                
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");
                
            return File.ReadAllText(filePath, Encoding.UTF8);
        }
        
        public static void WriteFile(string filePath, string content)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty");
                
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }
        
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
        
        public static long GetFileSize(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");
                
            return new FileInfo(filePath).Length;
        }
    }
}
