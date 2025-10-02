using System;
using System.IO;

namespace common_ops
{
    public static class SystemPaths
    {
        public static string GetCommonStartupFolderPath()
        {
            var programData = Environment.GetEnvironmentVariable("ProgramData");
            var commonStartupPath = Path.Combine(programData, @"Microsoft\Windows\Start Menu\Programs\Startup");
            return commonStartupPath;
        }
    }
}
