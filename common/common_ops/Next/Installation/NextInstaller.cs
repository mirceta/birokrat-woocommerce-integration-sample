using common_ops.diagnostics.Constants;
using common_ops.FileHandler;
using common_ops.Next.Executable;
using common_ops.Next.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common_ops.Next.Installation
{
    internal class NextInstaller : INextInstaller
    {
        private readonly FileVersionHelper _versionHelper;
        private readonly Action<string> _logger;
        private readonly IDirectoryContentHandler _directoryHandler;
        private readonly IExecutable _stopNext;
        private readonly FileBackup _fileBackup;
        private readonly bool _overwrite;

        public NextInstaller(
            Action<string> logger,
            IDirectoryContentHandler directoryHandler,
            IExecutable stopNext,
            FileBackup fileBackup,
            bool overwrite = false)
        {
            _logger = logger;
            _directoryHandler = directoryHandler;
            _stopNext = stopNext;
            _fileBackup = fileBackup;
            _overwrite = overwrite;
            _versionHelper = new FileVersionHelper();
        }

        public async Task<bool> TransferToArchive(string source, string deploymentName)
        {
            try
            {
                Directory.CreateDirectory(BiroLocationConstants.BironextLocalArchivePath);
                var archiveFolder = Path.Combine(BiroLocationConstants.BironextLocalArchivePath, deploymentName);

                _logger.Invoke(StartTransferMessage(source, archiveFolder));
                await _directoryHandler.CopyDirectoryAsync(source, archiveFolder);

                _logger.Invoke("Next transfer completed to archive");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Invoke("ERROR: Next transfer to archive failed!");
                throw;
            }
        }

        public async Task<bool> CreateBackupExe(string deploymentName)
        {
            var result = false;
            var task = Task.Run(() =>
            {
                result = CopyBirokratExeToExeArchive(_logger, deploymentName);
            });
            await task;

            return result;
        }

        public async Task<bool> TransferToLatest(string deploymentName)
        {
            try
            {
                var bironextLatest = BiroLocationConstants.BironextLocalLatestPath;
                Directory.CreateDirectory(bironextLatest);
                var archiveFolder = Path.Combine(BiroLocationConstants.BironextLocalArchivePath, deploymentName);

                await CopyFromArchive(bironextLatest, deploymentName, archiveFolder);
                LogInstalationDataInInfoFile(bironextLatest, deploymentName, archiveFolder);
                ReplaceWorkingExeWithNew(bironextLatest);

                _logger.Invoke("Next transfer completed to LATEST folder");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Invoke("ERROR: Next transfer to LATEST folder failed!");
                throw;
            }
        }

        public string GetPreviousDeployment(string sourceDirectory)
        {
            var path = Path.Combine(BiroLocationConstants.DefaultNextLocation, BiroLocationConstants.BironextInstallInfoFile);
            if (File.Exists(path))
            {
                var current = File.ReadAllLines(Path.Combine(BiroLocationConstants.DefaultNextLocation, BiroLocationConstants.BironextInstallInfoFile)).FirstOrDefault();

                if (current.Equals(sourceDirectory, StringComparison.OrdinalIgnoreCase))
                    return string.Empty;
                return current;
            }
            return sourceDirectory.Split('\\').Last();
        }

        private async Task CopyFromArchive(string bironextLatest, string folder, string archiveFolder)
        {
            _logger.Invoke(Environment.NewLine + "Stoping Next...");
            var (Result, Message) = await _stopNext.Execute();
            if (!Result)
                throw new Exception("Could not stop bironext");

            _logger.Invoke(Message);
            _logger.Invoke("Deleting '" + bironextLatest + "' content");

            //this will prevent deletion of next diagnostics if older deployment does not have it
            if (new DirectoryInfo(archiveFolder).GetDirectories().Any(x => x.Name.Equals("nextdiagnostics", StringComparison.OrdinalIgnoreCase)))
                _directoryHandler.DeleteAllContent(bironextLatest);
            else
                _directoryHandler.DeleteAllContent(bironextLatest, "nextdiagnostics");

            _logger.Invoke(StartTransferMessage(archiveFolder, bironextLatest));
            await _directoryHandler.CopyDirectoryAsync(archiveFolder, bironextLatest);
        }

        private void LogInstalationDataInInfoFile(string bironextLatest, string folder, string archiveFolder)
        {
            string version = _versionHelper.GetFileVersion(Path.Combine(archiveFolder, BiroLocationConstants.LocalRunnerGlobalPath)).ToString();

            var totalFiles = 0;
            var directories = Directory.GetDirectories(bironextLatest)
                .Where(x => BiroLocationConstants.NextFolders.Contains(x.Split('\\').Last()))
                .ToArray();

            foreach (var dir in directories)
            {
                var dirname = Path.GetFileName(dir);
                if (BiroLocationConstants.NextFolders.Any(x => x.Equals(dirname, StringComparison.OrdinalIgnoreCase)))
                    totalFiles += _directoryHandler.GetTotalFilesInDirectory(dir, ".txt");
            }

            File.WriteAllText(
                Path.Combine(bironextLatest, BiroLocationConstants.BironextInstallInfoFile),
                $"{folder}{Environment.NewLine}{version}{Environment.NewLine}{totalFiles}");
        }

        private void ReplaceWorkingExeWithNew(string bironextLatest)
        {
            _logger.Invoke("Copying Birokrat.exe from latest to " + BiroLocationConstants.BirokratDefaultLocation);
            var target = Path.Combine(bironextLatest, BiroLocationConstants.BirokratExeFileName);
            _fileBackup.Copy(target, Path.Combine(BiroLocationConstants.BirokratDefaultLocation));
        }

        private bool CopyBirokratExeToExeArchive(Action<string> logger, string deploymentFolderName)
        {
            logger?.Invoke("Creating a backup of Birokrat.exe to " + BiroLocationConstants.BirokratExeBackupFolder);
            var file = Path.Combine(BiroLocationConstants.BirokratDefaultLocation, BiroLocationConstants.BirokratExeFileName);
            try
            {
                if (!DoCopyBirokratExe(file, deploymentFolderName))
                {
                    logger?.Invoke("Backup of " + file + " already exists");
                    return true;
                }

                var result = _fileBackup.CopyToArchive(
                    file,
                    BiroLocationConstants.BirokratExeBackupFolder,
                    deploymentFolderName);

                if (result)
                    logger?.Invoke("File: '" + file + "' copied to: '" + BiroLocationConstants.BirokratExeBackupFolder + "' successfully");
                else
                    logger?.Invoke("Failed to backup File: '" + file + "' to: '" + BiroLocationConstants.BirokratExeBackupFolder + "'");

                return result;
            }
            catch
            {
                logger?.Invoke("File: '" + file + "' in '" + BiroLocationConstants.BirokratExeBackupFolder + "' already exists");
                return true;
            }
        }

        private bool DoCopyBirokratExe(string file, string deploymentName)
        {
            if (!Directory.Exists(BiroLocationConstants.BirokratExeBackupFolder))
                Directory.CreateDirectory(BiroLocationConstants.BirokratExeBackupFolder);

            var folders = Directory.GetDirectories(BiroLocationConstants.BirokratExeBackupFolder)
                .Where(x => x.Contains(deploymentName))
                .ToArray();

            if (!File.Exists(file))
                return false;

            var curVer = FileVersionInfo.GetVersionInfo(file);

            try
            {
                var doesAnyContainCurrentVersion = folders.Any(x =>
                {
                    var localFile = Path.Combine(x, BiroLocationConstants.BirokratExeFileName);
                    var backupVer = FileVersionInfo.GetVersionInfo(localFile);
                    return backupVer.FileVersion == curVer.FileVersion;
                });

                if (!doesAnyContainCurrentVersion)
                    return true;
            }
            catch
            {
                return true;
            }

            return false;
        }

        private string StartTransferMessage(string source, string target)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Starting next transfer");
            sb.AppendLine("From:  '" + source + "'");
            sb.AppendLine("To:    '" + target + "'");
            return sb.ToString();
        }
    }
}
