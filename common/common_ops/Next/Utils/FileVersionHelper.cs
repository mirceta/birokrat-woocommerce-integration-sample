using common_ops.diagnostics.Constants;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace common_ops.Next.Utils
{
    public class FileVersionHelper : IFileVersionHelper
    {
        public string GetLatestDeploymentFromSqlbirokrat_CompareToLocalVersion(string sqlBirokratDeplomentFolder, string localLatestFolder)
        {
            if (!Directory.Exists(sqlBirokratDeplomentFolder))
                throw new Exception("No access to sqlbirokrat!!");

            var latestRunnerGlobalPath = Path.Combine(Path.Combine(localLatestFolder, BiroLocationConstants.LocalRunnerGlobalPath));
            localLatestFolder = File.Exists(latestRunnerGlobalPath) ? latestRunnerGlobalPath : string.Empty;

            RunnerGlobalInfo localNextInfo = null;
            if (!string.IsNullOrEmpty(localLatestFolder))
                localNextInfo = BuildFileVersionInfo(localLatestFolder, latestRunnerGlobalPath);

            var vcomparer = new NextInstallVersionComparer();

            var latestFromSource = GetLatestDeploymentFromSqlbirokrat(sqlBirokratDeplomentFolder);

            var targetExePath = Path.Combine(latestFromSource, BiroLocationConstants.LocalRunnerGlobalPath);
            var sqlBirokratNextInfo = BuildFileVersionInfo(latestFromSource, targetExePath);

            if (localNextInfo != null)
            {
                vcomparer.CheckVersions(localNextInfo, sqlBirokratNextInfo);
                var source = vcomparer.GetLatestVersion();
                return string.IsNullOrEmpty(source) ? localLatestFolder : source;
            }
            else
                return latestFromSource;
        }

        public string GetLatestDeploymentFromSqlbirokrat(string sqlBirokratDeplomentFolder)
        {
            RunnerGlobalInfo localNextInfo = null;
            DirectoryInfo[] directories = default;

            try
            {
                directories = new DirectoryInfo(sqlBirokratDeplomentFolder).GetDirectories();
            }
            catch
            {
                return string.Empty;
            }
            var vcomparer = new NextInstallVersionComparer();

            Task task = Task.Run(() =>
            {
                foreach (var directory in directories)
                {
                    var targetExePath = Path.Combine(directory.FullName, BiroLocationConstants.LocalRunnerGlobalPath);
                    if (!File.Exists(targetExePath))
                        continue;

                    var sqlBirokratNextInfo = BuildFileVersionInfo(directory.FullName, targetExePath);
                    if (localNextInfo == null)
                    {
                        localNextInfo = sqlBirokratNextInfo;
                        continue;
                    }

                    vcomparer.CheckVersions(localNextInfo, sqlBirokratNextInfo);
                }
            });

            Task.WaitAll(task);
            return vcomparer.GetLatestVersion();
        }

        private RunnerGlobalInfo BuildFileVersionInfo(string root, string executablePath)
        {
            DateTime date;
            if (File.Exists(executablePath))
                date = new FileInfo(executablePath).LastWriteTime;
            else
                date = default;

            return new RunnerGlobalInfo(GetFileVersion(executablePath), date, root);
        }

        public Version GetFileVersion(string executablePath)
        {
            FileVersionInfo versionInfo;
            if (File.Exists(executablePath))
                versionInfo = FileVersionInfo.GetVersionInfo(executablePath);
            else
                return new Version("1.0.0.0");

            return new Version(string.IsNullOrEmpty(versionInfo.ProductVersion) ? "1.0.0.0" : versionInfo.ProductVersion);
        }
    }
}
