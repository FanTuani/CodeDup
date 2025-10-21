using System.Collections.Generic;
using CodeDup.Core.Models;

namespace CodeDup.Core.Storage
{
    public interface IProjectStore
    {
        IEnumerable<string> ListProjects();
        bool CreateProject(string projectName);
        bool DeleteProject(string projectName);

        IEnumerable<CodeFileMetadata> ListFiles(string projectName);
        CodeFileMetadata? GetFile(string projectName, string fileId);
        bool RemoveFile(string projectName, string fileId);
        CodeFileMetadata AddFile(string projectName, string originalFilePath, bool overwriteIfNameExists, out bool skippedAsDuplicate);
        string GetFileContentPath(string projectName, string fileId);
    }
}


