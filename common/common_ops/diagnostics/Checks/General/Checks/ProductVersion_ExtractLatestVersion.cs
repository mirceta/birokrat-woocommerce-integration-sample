using common_ops.diagnostics.Checks.General.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.General.Checks
{
    /// <summary>
    /// Compares the version of local program files against each other to determine the latest version.
    /// This class processes a list of program files, extracts their versions, and can optionally return
    /// only the latest version among them.
    /// <para>The operation involves iterating through provided file paths, extracting the version from
    /// each file using <see cref="FileVersionExtractor"/>, and either listing all versions or determining
    /// the highest version. If no valid files are provided or all provided paths are invalid, it returns
    /// an error.</para>
    /// <para>Results:</para>
    /// <list type="bullet">
    /// <item><description>If <see cref="_extractOnlyLatest"/> is true, returns the path and version of the file with the latest version.</description></item>
    /// <item><description>If <see cref="_extractOnlyLatest"/> is false, returns a list of all versions extracted, prefixed with their respective paths.</description></item>
    /// <item><description>Returns an error if no files could be processed.</description></item>
    /// </list>
    /// </summary>
    public class ProductVersion_ExtractLatestVersion : ICheck
    {
        private readonly IFileVersionExtractor _fileVerExtractor;
        private readonly string[] _files;
        private readonly bool _extractOnlyLatest;

        /// <summary>
        /// <inheritdoc cref="ProductVersion_ExtractLatestVersion"/>
        /// </summary>
        /// <param name="files">An array of file paths for which the versions will be extracted and compared.</param>
        /// <param name="extractOnlyLatest">A Boolean value indicating whether to return only the latest version among the provided files.</param>
        public ProductVersion_ExtractLatestVersion(IFileVersionExtractor fileVerExtractor, string[] files, bool extractOnlyLatest)
        {
            _fileVerExtractor = fileVerExtractor;
            _files = files;
            _extractOnlyLatest = extractOnlyLatest;
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
            var info = new List<string>();

            if (_files.Count() == 0)
                return new ResultRecord(false, GetType().Name, new string[] { "No files provided" + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR });

            var infoLoc = await GetVersionsAsync(info);

            if (_extractOnlyLatest && info.Count() > 0)
            {
                var latest = infoLoc.OrderBy(x => x.Version).Last();
                return new ResultRecord(true, GetType().Name, new string[] { latest.Path + TextConstants.DELIMITER + latest.Version });
            }

            if (infoLoc.Count() == 0)
                return new ResultRecord(false, GetType().Name, new string[] { TextConstants.POSTFIX_ERROR });

            return new ResultRecord(true, GetType().Name, infoLoc.OrderBy(x => x.Version).Select(x => x.Path + TextConstants.DELIMITER + x.Version).ToArray());
        }

        private async Task<List<(string Path, string Version)>> GetVersionsAsync(List<string> info)
        {
            return await Task.Run(() =>
            {
                var infoLoc = new List<(string Path, string Version)>();
                foreach (var file in _files)
                {
                    if (!File.Exists(file))
                        continue;

                    if (!_fileVerExtractor.TryGetVersion(out var sourceVersion, file))
                    {
                        continue;
                    }

                    infoLoc.Add((file, info.Last()));
                }

                return infoLoc;
            });
        }

        public async Task<string[]> VersionsAsync(string sourceProgramBasePath, string programName)
        {
            return await Task.Run(() =>
            {
                return Directory.EnumerateFiles(sourceProgramBasePath, programName, SearchOption.AllDirectories).ToArray();
            });
        }

        private void SafetyChecksFile(List<string> info, string path)
        {
            if (!File.Exists(path))
                info.Add("Wrong path provided: " + path + "||" + TextConstants.POSTFIX_ERROR);
        }

        private void SafetyChecksDirectory(List<string> info, string path)
        {
            if (!Directory.Exists(path))
                info.Add("Wrong path provided: " + path + "||" + TextConstants.POSTFIX_ERROR);
        }
    }
}
