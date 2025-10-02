using common_ops.diagnostics.Constants;
using common_ops.FileHandler;
using System;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.General.Checks
{
    /// <summary>
    /// Will compare the file count between source and origin directory. If file count matches result will be true. Class allows to exclude specific names or specifix extensions.
    /// </summary>
    /// <remarks>
    /// the local version. <see cref="ResultRecord.AdditionalInfo"/> contains: file count for origin and source separated with || (<see cref="TextConstants.DELIMITER"/>)
    /// </remarks>
    public class FileCountVerifier_Check : ICheck
    {
        private readonly IDirectoryContentHandler _contentHandler;
        private readonly string _sourceDirectory;
        private readonly string _originDirectory;
        private readonly string[] _filesToExclude;

        /// <summary>
        /// <inheritdoc cref="FileCountVerifier_Check"/>
        /// </summary>
        public FileCountVerifier_Check(IDirectoryContentHandler contentHandler, string sourceDirectory, string originDirectory, params string[] filesToExclude)
        {
            _contentHandler = contentHandler;
            _sourceDirectory = sourceDirectory;
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
                var sourceFileCount = _contentHandler.GetTotalFilesInDirectory(_sourceDirectory, _filesToExclude);
                var originFileCount = _contentHandler.GetTotalFilesInDirectory(_originDirectory, _filesToExclude);

                var result = sourceFileCount == originFileCount;

                var info = new string[]
                {
                    "source" + TextConstants.DELIMITER + sourceFileCount.ToString(),
                    "origin" + TextConstants.DELIMITER + originFileCount.ToString()
                };

                return new ResultRecord(result, GetType().Name, info);
            });

            var record = await task;
            return record;
        }
    }
}
