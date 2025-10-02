using common_ops.Abstractions;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.General.Checks
{
    /// <summary>
    /// Performs a version comparison between a local program and multiple versions of the same program 
    /// located at a specified source path. Program name will be extracted from localProgramPath. This
    /// check will iterate through all matching program _files in the source directory and compare each one against
    /// the local version.
    /// <para>This check is designed to ensure that the local program version is not less than any of the versions 
    /// found in the source path. It returns a negative result (false) if any source version is greater than
    /// the local version. <see cref="ResultRecord.AdditionalInfo"/> contains: version||OK if local version is
    /// the latest or equal to the source _files. WARNING if no source _files can be detected. If there is a file
    /// with newer version it will return sourceLocation||ERROR. </para>
    /// </summary>
    public class ProductVersion_CompareCheck_Multiple : ICheck
    {
        private readonly Func<string, string, ProductVersion_CompareCheck_Single> _simpleVerCompareFactory;
        private readonly IDirectorySystem _directorySystem;
        private readonly IFileSystem _fileSystem;
        private readonly string _localProgramPath;
        private readonly string _sourceRootDirectory;

        /// <summary>
        /// <inheritdoc cref="ProductVersion_CompareCheck_Multiple"/>
        /// </summary>
        /// <param name="localProgramPath">The path to the local program whose version is to be checked.
        /// Program name to be checked will be determined from this path</param>
        /// <param name="sourceRootDirectory">The base path where multiple versions of the program are
        /// stored for comparison.</param>
        public ProductVersion_CompareCheck_Multiple(
            Func<string, string, ProductVersion_CompareCheck_Single> simpleVerCompareFactory,
            IDirectorySystem directorySystem,
            IFileSystem fileSystem,
            string localProgramPath,
            string sourceRootDirectory)
        {
            _simpleVerCompareFactory = simpleVerCompareFactory;
            _directorySystem = directorySystem;
            _fileSystem = fileSystem;
            _localProgramPath = localProgramPath;
            _sourceRootDirectory = sourceRootDirectory;
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
            var programName = Path.GetFileName(_localProgramPath);

            var info = new List<string>();

            var checkFile = SafetyChecksFile(_localProgramPath);
            if (!string.IsNullOrEmpty(checkFile)) info.Add(checkFile);

            var checkDirectory = SafetyChecksDirectory(_sourceRootDirectory);
            if (!string.IsNullOrEmpty(checkDirectory)) info.Add(checkDirectory);

            if (info.Count > 0)
                return new ResultRecord(false, GetType().Name, info.ToArray());

            var files = await GetFilesAsync(_sourceRootDirectory, programName);

            if (files.Length == 0)
                return new ResultRecord(true, GetType().Name, "No files in root directory" + TextConstants.DELIMITER + TextConstants.POSTFIX_WARNING);

            var latestFileSource = string.Empty;
            if (files.Length > 1)
            {
                var results = new List<(string File, ResultRecord Record)>();
                foreach (var file in files)
                {
                    var sourceCheck = await _simpleVerCompareFactory(_localProgramPath, file).Run();
                    if (!sourceCheck.Result)
                        results.Add((file, sourceCheck));
                }

                if (results.Count == 0)
                    latestFileSource = _localProgramPath;
                else
                    latestFileSource = results.OrderBy(x => x.Record.AdditionalInfo[1]).Last().File;
            }
            else
                latestFileSource = files.First();

            var versionCheck = await _simpleVerCompareFactory(_localProgramPath, latestFileSource).Run();
            if (!versionCheck.Result)
                return new ResultRecord(false, GetType().Name, latestFileSource + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR);

            return new ResultRecord(true, GetType().Name, versionCheck.AdditionalInfo.FirstOrDefault() + TextConstants.DELIMITER + TextConstants.POSTFIX_OK);
        }

        public async Task<string[]> GetFilesAsync(string sourceProgramBasePath, string programName)
        {
            return await Task.Run(() =>
            {
                return _directorySystem.EnumerateFiles(sourceProgramBasePath, programName, SearchOption.AllDirectories).ToArray();
            });
        }

        private string SafetyChecksFile(string path)
        {
            if (!_fileSystem.Exists(path))
                return "Wrong path provided: " + path + "||" + TextConstants.POSTFIX_ERROR;

            return string.Empty;
        }

        private string SafetyChecksDirectory(string path)
        {
            if (!_directorySystem.Exists(path))
                return "Wrong directory provided: " + path + "||" + TextConstants.POSTFIX_ERROR;

            return string.Empty;
        }
    }
}
