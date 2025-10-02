using System.IO;

namespace common_ops.Abstractions
{
    public class PathSystem : IPathSystem
    {
        public string GetFileExtension(string file)
        {
            return Path.GetExtension(file);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }
    }
}
