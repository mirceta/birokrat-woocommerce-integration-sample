using System.Collections.Generic;
using System.IO;

namespace common_ops.Abstractions
{
    public class FileSystem : IFileSystem
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string pathToFile)
        {
            return File.ReadAllText(pathToFile);
        }

        public long GetFileSize(string file)
        {
            return GetFileInfo(file).Length;
        }

        public string GetFileNameWithoutExtension(string file)
        {
            return Path.GetFileNameWithoutExtension(file);
        }

        public void Delete(string file)
        {
            File.Delete(file);
        }

        public FileInfo GetFileInfo(string file)
        {
            return new FileInfo(file);
        }

        public void WriteAllLines(string location, List<string> content)
        {
            File.WriteAllLines(location, content);
        }
    }
}
