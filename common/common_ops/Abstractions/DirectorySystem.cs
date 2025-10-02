using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace common_ops.Abstractions
{
    public class DirectorySystem : IDirectorySystem
    {
        public DirectoryInfo[] GetDirectoriesInfo(string location)
        {
            return new DirectoryInfo(location).GetDirectories();
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public bool IsAnyDirectory(string directoryPath)
        {
            return new DirectoryInfo(directoryPath).GetDirectories().Any();
        }

        public void Delete(string file, bool recursive = false)
        {
            Directory.Delete(file, recursive);
        }

        public string[] GetFiles(string path, string searchPattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return Directory.GetFiles(path, searchPattern, option);
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return Directory.EnumerateFiles(path);
        }

        public string GetDirectoryNameFromPath(string path)
        {
            var normalized = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (Directory.Exists(normalized))
                return normalized.Split(Path.DirectorySeparatorChar).Last();

            var tmp = normalized.Split(Path.DirectorySeparatorChar);
            return tmp[tmp.Length - 2];
        }
    }
}
