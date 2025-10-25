using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CodeDup.Core.Models;

namespace CodeDup.Core.Storage
{
    public class FileProjectStore : IProjectStore
    {
        private readonly string _rootDir;

        public FileProjectStore(string rootDir)
        {
            _rootDir = rootDir;
            Directory.CreateDirectory(_rootDir);
        }

        public IEnumerable<string> ListProjects()
        {
            if (!Directory.Exists(_rootDir)) yield break;
            foreach (var dir in Directory.GetDirectories(_rootDir))
            {
                yield return Path.GetFileName(dir);
            }
        }

        public bool CreateProject(string projectName)
        {
            var dir = GetProjectDir(projectName);
            if (Directory.Exists(dir)) return false;
            Directory.CreateDirectory(dir);
            Directory.CreateDirectory(Path.Combine(dir, "files"));
            return true;
        }

        public bool DeleteProject(string projectName)
        {
            var dir = GetProjectDir(projectName);
            if (!Directory.Exists(dir)) return false;
            Directory.Delete(dir, true);
            return true;
        }

        public IEnumerable<CodeFileMetadata> ListFiles(string projectName)
        {
            var metaPath = GetMetaPath(projectName);
            if (!File.Exists(metaPath)) yield break;
            var items = JsonSerializer.Deserialize<List<CodeFileMetadata>>(File.ReadAllText(metaPath)) ?? new List<CodeFileMetadata>();
            foreach (var m in items) yield return m;
        }

        public CodeFileMetadata? GetFile(string projectName, string fileId)
        {
            return ListFiles(projectName).FirstOrDefault(f => f.Id == fileId);
        }

        public bool RemoveFile(string projectName, string fileId)
        {
            var items = ListFiles(projectName).ToList();
            var removed = items.RemoveAll(f => f.Id == fileId) > 0;
            if (!removed) return false;
            SaveMeta(projectName, items);
            var contentPath = GetFileContentPath(projectName, fileId);
            if (File.Exists(contentPath)) File.Delete(contentPath);
            return true;
        }

        public CodeFileMetadata AddFile(string projectName, string originalFilePath, bool overwriteIfNameExists, out bool skippedAsDuplicate)
        {
            skippedAsDuplicate = false;
            var items = ListFiles(projectName).ToList();
            var fileName = Path.GetFileName(originalFilePath);
            var ext = Path.GetExtension(originalFilePath).TrimStart('.').ToLowerInvariant();
            var existing = items.FirstOrDefault(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (existing != null && !overwriteIfNameExists)
            {
                skippedAsDuplicate = true;
                return existing;
            }

            var metadata = existing ?? new CodeFileMetadata();
            metadata.FileName = fileName;
            metadata.Extension = ext;
            metadata.ProjectName = projectName;
            metadata.ImportedAt = DateTime.UtcNow;
            metadata.FileSizeBytes = new FileInfo(originalFilePath).Length;
            metadata.RelativePath = $"files/{metadata.Id}.txt";
            
            // 设置编程语言
            metadata.ProgrammingLanguage = LanguageDetect.FromExtension(ext);

            var dest = Path.Combine(GetProjectDir(projectName), metadata.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(originalFilePath, dest, overwriteIfNameExists);

            if (existing == null)
            {
                items.Add(metadata);
            }
            SaveMeta(projectName, items);
            return metadata;
        }

        public string GetFileContentPath(string projectName, string fileId)
        {
            return Path.Combine(GetProjectDir(projectName), "files", fileId + ".txt");
        }

        private string GetProjectDir(string projectName) => Path.Combine(_rootDir, projectName);

        private string GetMetaPath(string projectName) => Path.Combine(GetProjectDir(projectName), "metadata.json");

        private void SaveMeta(string projectName, List<CodeFileMetadata> items)
        {
            var metaPath = GetMetaPath(projectName);
            Directory.CreateDirectory(Path.GetDirectoryName(metaPath)!);
            File.WriteAllText(metaPath, JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}


