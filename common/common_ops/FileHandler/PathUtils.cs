using System;
using System.IO;

namespace common_ops.FileHandler
{
    public static class PathUtils
    {
        public static string GetDirectoryName(string path)
        {
            try
            {
                if (File.Exists(path))
                    return Path.GetDirectoryName(path);

                if (Directory.Exists(path))
                    return path;

                throw new Exception("Invalid path");
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., security issues, invalid path formats)
                return "Error determining path '" + path + "' type: " + ex.Message;
            }
        }
    }
}
