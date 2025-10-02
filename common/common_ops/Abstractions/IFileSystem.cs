using System.Collections.Generic;
using System.IO;

namespace common_ops.Abstractions
{
    public interface IFileSystem
    {
        bool Exists(string path);
        string ReadAllText(string pathToFile);
        void Delete(string file);
        FileInfo GetFileInfo(string file);
        void WriteAllLines(string location, List<string> content);
        long GetFileSize(string file);
    }
}
