using System;
using System.Diagnostics;
using System.Linq;

namespace common_ops.diagnostics.Checks.General.Utils
{
    internal class FileVersionExtractor : IFileVersionExtractor
    {
        public bool TryGetVersion(out Version version, string path)
        {
            FileVersionInfo fver = FileVersionInfo.GetVersionInfo(path);

            var versions = new string[]
            {
                CleanVersion(fver.FileMajorPart + "." + fver.FileMinorPart + "." + fver.FileBuildPart),
                CleanVersion(fver.ProductMajorPart + "." + fver.ProductMinorPart + "." + fver.ProductBuildPart)
            };

            if (versions.All(x => string.IsNullOrEmpty(x)))
            {
                version = default;
                return false;
            }

            version = new Version(versions.OrderBy(x => x).FirstOrDefault());
            return true;
        }

        private string CleanVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                return version;

            return version.Split('+')[0].Trim();
        }
    }
}
