using System;
using System.IO;
using System.Text;

namespace common_ops.FileHandler
{
    public class FileBackup
    {
        private readonly bool _overwrite;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBackup"/> class.
        /// </summary>
        /// <param name="overwrite">Specifies whether to overwrite existing _files in the backup directory.</param>
        public FileBackup(bool overwrite = false)
        {
            _overwrite = overwrite;
        }

        /// <summary>
        /// Copies the specified _files to the backup directory.
        /// </summary>
        /// <param name="fileToCopy">path to file to copy.</param>
        /// <param name="archiveDirectoryPath">The path to the directory where backups will be stored.</param>
        /// <param name="backupSubfolderName">The name of the subfolder for the backup. If empty, only a timestamp-based name will be used.</param>
        /// <returns><c>true</c> if all _files were successfully copied; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// If <paramref name="filesToCopy"/> contains _files that do not exist, result will be false. Files that exist will be copied sussessfully
        /// Existing _files in the backup directory will be overwritten if <see cref="_overwrite"/> is set to <c>true</c>.
        /// </remarks>
        public bool CopyToArchive(string fileToCopy, string archiveDirectoryPath, string backupSubfolderName = "")
        {
            var backupPath = GenerateAndReturnBackupFolderPath(archiveDirectoryPath, backupSubfolderName);

            if (File.Exists(fileToCopy))
            {
                var fileName = Path.GetFileName(fileToCopy);
                File.Copy(fileToCopy, Path.Combine(backupPath, fileName), _overwrite);
            }
            else
                return false;

            return true;
        }

        /// <summary>
        /// Copies the specified _files to the backup directory.
        /// </summary>
        /// <param name="filesToCopy">An array of file paths to copy.</param>
        /// <param name="archiveDirectoryPath">The path to the directory where backups will be stored.</param>
        /// <param name="backupSubfolderName">The name of the subfolder for the backup. If empty, only a timestamp-based name will be used.</param>
        /// <returns><c>true</c> if all _files were successfully copied; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// If <paramref name="filesToCopy"/> contains _files that do not exist, result will be false. Files that exist will be copied sussessfully
        /// Existing _files in the backup directory will be overwritten if <see cref="_overwrite"/> is set to <c>true</c>.
        /// </remarks>
        public bool CopyToArchive(string[] filesToCopy, string archiveDirectoryPath, string backupSubfolderName = "")
        {
            var backupPath = GenerateAndReturnBackupFolderPath(archiveDirectoryPath, backupSubfolderName);
            var result = true;

            foreach (var file in filesToCopy)
            {
                if (File.Exists(file))
                {
                    var fileName = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(backupPath, fileName), _overwrite);
                }
                else
                {
                    result = false;
                    continue;
                }
            }

            return result;
        }

        private string GenerateAndReturnBackupFolderPath(string archiveFullName, string backupFolderName = "")
        {
            var backupPath = Path.Combine(archiveFullName, CreateBackupFolderName(backupFolderName));
            Directory.CreateDirectory(backupPath);
            return backupPath;
        }

        private string CreateBackupFolderName(string deploymentName)
        {
            var now = DateTime.Now;
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(deploymentName))
                sb.Append(deploymentName);
            else
                sb.Append(now.ToString("yyyy-MM-dd"));

            sb.Append("_");
            sb.Append(now.Hour < 10 ? "0" + now.Hour : now.Hour.ToString());
            sb.Append(now.Minute < 10 ? "0" + now.Minute : now.Minute.ToString());
            sb.Append(now.Second < 10 ? "0" + now.Second : now.Second.ToString());

            return sb.ToString();
        }

        public bool Copy(string fileToCopy, string targetFolder, bool overwrite = true)
        {
            if (!File.Exists(fileToCopy))
                return false;

            try
            {
                var fileName = Path.GetFileName(fileToCopy);
                File.Copy(
                    fileToCopy,
                    Path.Combine(targetFolder, fileName),
                    overwrite);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
