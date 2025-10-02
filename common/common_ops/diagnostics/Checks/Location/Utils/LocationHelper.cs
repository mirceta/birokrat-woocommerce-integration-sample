using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace common_ops.diagnostics.Checks.Location.Utils
{
    public class LocationHelper : ILocationHelper
    {
        public (string location, bool result) CheckIfFolderExists(string location, string defaultLocation)
        {
            if (Directory.Exists(location))
                return (location, true);
            if (Directory.Exists(defaultLocation))
                return (defaultLocation, true);
            return (location, false);
        }

        public bool IsWritePermissionGranted(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }
            var file = "test123dowehaveaccess.txt";
            var local_path = Path.Combine(path, file);
            try
            {
                if (File.Exists(local_path))
                    File.Delete(local_path);

                File.WriteAllText(local_path, ":)");
                File.Delete(local_path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsReadPermissionGranted(string path)
        {
            try
            {
                var files = Directory.GetFiles(path);
                if (files.Length > 0)
                {
                    using (File.OpenRead(files[0])) { }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public (bool Result, string[] CheckInfo) AreAllRequiredFilesPresent(FileInfo[] localFiles, params string[] requiredFiles)
        {
            return Check(localFiles.Select(x => x.Name.Split('.').FirstOrDefault()).ToArray(), requiredFiles, "files");
        }

        public (bool Result, string[] CheckInfo) AreAllRequiredFoldersPresent(DirectoryInfo[] localFolders, params string[] requiredFolders)
        {
            return Check(localFolders.Select(x => x.Name).ToArray(), requiredFolders, "folders");
        }

        public (bool Result, string[] CheckInfo) Check(string[] local, string[] required, string mode)
        {
            if (local.Length == 0)
                return (false, new string[] { $"No {mode} in location!" });

            List<string> missing = new List<string>();

            foreach (var req in required)
            {
                if (!local.Any(x => x.Equals(req, StringComparison.OrdinalIgnoreCase)))
                    missing.Add(req);
            }

            return (!missing.Any(), missing.ToArray());
        }
    }
}
