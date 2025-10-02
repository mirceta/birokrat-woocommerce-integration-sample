using common_ops.diagnostics.Checks.General.Checks;
using common_ops.diagnostics.Constants;
using common_ops.FileHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Next.Checks
{
    /// <summary>
    /// Will compare the file count between local and origin directory. It will only count folders that are required for Next to run (as defined
    /// in <see cref="BiroLocationConstants.NextFolders"/>) and appsettings.json in base folder. If both file counts matches result will be true.
    /// Class allows to exclude specific names or specific extensions. In this example it is advised to exclude .txt _files (logs).
    /// </summary>
    /// <remarks>
    /// the local version. <see cref="ResultRecord.AdditionalInfo"/> contains: file count for origin and local separated with ||
    /// (<see cref="TextConstants.DELIMITER"/>)
    /// </remarks>
    public class BiroNext_FileCountVerifier_Check : ICheck
    {
        private readonly IDirectoryContentHandler _contentHandler;
        private readonly string _localDirectory;
        private readonly string _originDirectory;
        private readonly string[] _filesToExclude;

        /// <summary>
        /// <inheritdoc cref="FileCountVerifier_Check"/>
        /// </summary>
        public BiroNext_FileCountVerifier_Check(IDirectoryContentHandler contentHandler, string localDirectory, string originDirectory, params string[] filesToExclude)
        {
            _contentHandler = contentHandler;
            _localDirectory = localDirectory;
            _originDirectory = originDirectory;
            _filesToExclude = filesToExclude;
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                return await Work();
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
        }

        private async Task<ResultRecord> Work()
        {
            var task = Task.Run(() =>
            {
                var localFileCount = CountFilesInNextSubfolders(_contentHandler, _localDirectory, _filesToExclude);
                var originFileCount = CountFilesInNextSubfolders(_contentHandler, _originDirectory, _filesToExclude);

                var result = localFileCount >= originFileCount;

                var info = new List<string>
                {
                    "Local" + TextConstants.DELIMITER + localFileCount.ToString(),
                    "Origin" + TextConstants.DELIMITER + originFileCount.ToString()
                };

                if (!result)
                {
                    var localFiles = _contentHandler
                        .GetAllFilesInDirectory(_localDirectory, _filesToExclude)
                        .Select(x => x.Replace(_localDirectory, ""))
                        .ToArray();

                    var originFiles = _contentHandler
                        .GetAllFilesInDirectory(_originDirectory, _filesToExclude)
                        .Select(x => x.Replace(_originDirectory, ""))
                        .ToArray();

                    var diff = originFiles.Except(localFiles).ToArray();

                    foreach (var df in diff)
                        info.Add("File Missing: '" + df + "'" + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR);
                }

                return new ResultRecord(result, GetType().Name, info.ToArray());
            });

            var record = await task;

            return record;
        }

        private int CountFilesInNextSubfolders(IDirectoryContentHandler dirHandler, string basePath, params string[] filesToExclude)
        {
            var count = 0;

            foreach (var dir in BiroLocationConstants.NextFolders)
                count += dirHandler.GetTotalFilesInDirectory(Path.Combine(basePath, dir), _filesToExclude);

            if (basePath.Contains(BiroNextConstants.NextSettingsFileName))
                count++;

            if (basePath.Contains(BiroLocationConstants.BirokratExeFileName))
                count++;

            return count;
        }
    }
}
