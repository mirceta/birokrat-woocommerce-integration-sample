using System;

namespace common_ops.Next.Utils
{
    internal class RunnerGlobalInfo
    {
        internal Version ProductVersion;
        internal DateTime ModifiedDate;
        internal string RootPath;

        public RunnerGlobalInfo(Version productVersion, DateTime modifiedDate, string rootPath)
        {
            ProductVersion = productVersion;
            ModifiedDate = modifiedDate;
            RootPath = rootPath;
        }
    }
}
