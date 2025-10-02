using common_ops.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace common_ops.FileHandler
{
    public class DirectoryContentHandler : IDirectoryContentHandler
    {
        private readonly IProgress<double> _progress;
        private readonly IFileSystem _fileSystem;
        private readonly IDirectorySystem _directorySystem;
        private readonly IPathSystem _pathSystem;
        private readonly ICopyFileWithProgress _copyFileWithProgress;
        private readonly int _progressIntervalInMilliseconds;

        private double _progressPercent;
        private long _totalBytes = 0;

        public DirectoryContentHandler(
            IFileSystem fileSystem,
            IDirectorySystem directorySystem,
            IPathSystem pathSystem,
            ICopyFileWithProgress copyFileWithProgress,
            Action<string> logger,
            int progressIntervalInMilliseconds = 1000)
        {
            _progress = new Progress<double>(percent =>
            {
                logger?.Invoke("Progress: " + percent.ToString("F2"));
            });

            _fileSystem = fileSystem;
            _directorySystem = directorySystem;
            _pathSystem = pathSystem;
            _copyFileWithProgress = copyFileWithProgress;
            _progressIntervalInMilliseconds = progressIntervalInMilliseconds;
        }

        public async Task CopyDirectoryAsync(string sourceDir, string targetDir, bool overwrite = true)
        {
            if (!_directorySystem.Exists(sourceDir))
            {
                throw new DirectoryNotFoundException("Source directory not found: " + sourceDir);
            }
            _totalBytes = GetDirectorySize(sourceDir);
            _copyFileWithProgress.Reset();

            _directorySystem.CreateDirectory(targetDir);
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            var report = Task.Run(() => ProgressReportUpdate(token), token);

            try
            {
                await CopyFilesAsync(sourceDir, targetDir);

                List<Task> tasks = new List<Task>();
                var directories = _directorySystem.GetDirectories(sourceDir);
                foreach (var dir in directories)
                {
                    string targetSubDir = Path.Combine(targetDir, _pathSystem.GetFileName(dir));
                    tasks.Add(CopyDirectoriesRecursiveAsync(dir, targetSubDir, overwrite));
                }
                await Task.WhenAll(tasks);
                cancellationTokenSource.Cancel();
            }
            catch
            {
                cancellationTokenSource?.Cancel();
            }
        }

        private async Task CopyDirectoriesRecursiveAsync(string sourceDir, string targetDir, bool overwrite)
        {
            if (!_directorySystem.Exists(sourceDir))
                throw new DirectoryNotFoundException("Source directory not found: " + sourceDir);

            _directorySystem.CreateDirectory(targetDir);

            await CopyFilesAsync(sourceDir, targetDir);

            var directories = _directorySystem.GetDirectories(sourceDir);
            foreach (var directory in directories)
            {
                string targetSubDir = Path.Combine(targetDir, _pathSystem.GetFileName(directory));
                await CopyDirectoriesRecursiveAsync(directory, targetSubDir, overwrite);
            }
        }

        private async Task CopyFilesAsync(string sourceDir, string targetDir)
        {
            var files = _directorySystem.GetFiles(sourceDir);
            var tasks = new List<Task>();
            using (var semaphore = new SemaphoreSlim(5))
            {
                foreach (var file in files)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            string targetFilePath = Path.Combine(targetDir, _pathSystem.GetFileName(file));
                            await _copyFileWithProgress.Copy(file, targetFilePath);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// Deletes ALL folder content. Enables to exclude some _files. Will only exclude _files or folders that are in root (<paramref name="folderPath"/>)
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="excluded"></param>
        /// <returns></returns>
        public List<string> DeleteAllContent(string folderPath, params string[] excluded)
        {
            if (!_directorySystem.Exists(folderPath))
            {
                Console.WriteLine("The directory '" + folderPath + "' does not exist.");
                return default;
            }
            var log = new List<string>();
            var files = _directorySystem.GetFiles(folderPath);
            foreach (var file in files)
            {
                if (excluded.Any(x => x.Equals(_pathSystem.GetFileNameWithoutExtension(file), StringComparison.OrdinalIgnoreCase)))
                    continue;

                try
                {
                    _fileSystem.Delete(file);
                    log.Add("Deleted file: " + file);
                }
                catch (Exception ex)
                {
                    log.Add("Failed to delete file: " + file + ". Error: " + ex.Message);
                }
            }

            var directories = _directorySystem.GetDirectories(folderPath);
            foreach (var directory in directories)
            {
                if (excluded.Any(x => x.Equals(directory.Split('\\').Last(), StringComparison.OrdinalIgnoreCase)))
                    continue;

                try
                {
                    _directorySystem.Delete(directory, true); // 'true' ensures recursive deletion
                    log.Add("Deleted directory: " + directory);
                }
                catch (Exception ex)
                {
                    log.Add("Failed to delete directory: " + directory + ". Error: " + ex.Message);
                }
            }
            return log;
        }

        private long GetDirectorySize(string dirPath)
        {
            long size = 0;
            var files = _directorySystem.GetFiles(dirPath);
            foreach (var file in files)
            {
                size += _fileSystem.GetFileSize(file);
            }

            var directories = _directorySystem.GetDirectories(dirPath);
            foreach (var directory in directories)
            {
                size += GetDirectorySize(directory);
            }
            return size;
        }

        private async Task ProgressReportUpdate(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_progressIntervalInMilliseconds, token);
                    double progressPercentage = (double)_copyFileWithProgress.BytesCopied / _totalBytes * 100;
                    if (progressPercentage - _progressPercent > 5f)
                    {
                        _progressPercent = progressPercentage;
                        _progress.Report(progressPercentage);
                    }
                } catch (TaskCanceledException ex) { }
            }
        }

        public int GetTotalFilesInDirectory(string directoryPath, params string[] filesToExclude)
        {
            if (!_directorySystem.Exists(directoryPath))
                return 0;

            if (!_directorySystem.IsAnyDirectory(directoryPath))
                return 0;

            int CountFilesRecursive(string directory, int amount)
            {
                var files = _directorySystem.EnumerateFiles(directory);

                foreach (var pattern in filesToExclude)
                    files = files.Where(file => IsFileValid(file, filesToExclude));
                amount = files.Count();

                var directories = _directorySystem.GetDirectories(directory);

                foreach (var d in directories)
                {
                    amount += CountFilesRecursive(d, amount);
                }
                return amount;
            }

            return CountFilesRecursive(directoryPath, 0);
        }

        public string[] GetAllFilesInDirectory(string directoryPath, params string[] filesToExclude)
        {
            if (!_directorySystem.Exists(directoryPath))
                return Array.Empty<string>();

            if (!_directorySystem.IsAnyDirectory(directoryPath))
                return Array.Empty<string>();

            var files = _directorySystem.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            files = files.Where(x => IsFileValid(x, filesToExclude)).ToArray();

            return files;
        }

        private bool IsFileValid(string fileName, string[] filter)
        {
            var nameRaw = _pathSystem.GetFileNameWithoutExtension(fileName);
            var name = _pathSystem.GetFileNameWithoutExtension(fileName);
            var extension = _pathSystem.GetFileExtension(fileName);

            if (filter.Any(x => x.Equals(nameRaw, StringComparison.OrdinalIgnoreCase)))
                return false;
            if (filter.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return false;
            if (filter.Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase)))
                return false;
            return true;
        }
    }
}
