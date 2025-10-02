using System.Collections.Generic;
using System.IO;

namespace common_ops.Abstractions
{
    public interface IDirectorySystem
    {
        DirectoryInfo[] GetDirectoriesInfo(string location);
        bool Exists(string path);
        void CreateDirectory(string path);
        string[] GetDirectories(string path);
        bool IsAnyDirectory(string directoryPath);
        void Delete(string file, bool recursive = false);
        string[] GetFiles(string path, string searchPattern = "*", SearchOption option = SearchOption.TopDirectoryOnly);
        IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*", SearchOption option = SearchOption.TopDirectoryOnly);
        string GetDirectoryNameFromPath(string path);
    }
}
