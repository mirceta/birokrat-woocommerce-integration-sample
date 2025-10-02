using common_ops.Abstractions;
using common_ops.diagnostics.Checks.General.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.General.Checks
{
    /// <summary>
    /// Checks program version and compare it to source program version. Both paths need to be provided. Will return false
    /// if  sourceVersion is larger than local version
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains:
    /// <para>localVersion</para>
    /// <para>sourceVersion</para>
    /// Separated with <c>||</c> in case of an ERROR</para>
    /// </summary>
    public class ProductVersion_CompareCheck_Single : ICheck
    {
        private readonly IFileVersionExtractor _fileVerExtractor;
        private readonly IFileSystem _fileSystem;
        private readonly string _localProgram;
        private readonly string _sourceProgram;

        /// <summary>
        /// <inheritdoc cref="ProductVersion_CompareCheck_Single"/>
        /// </summary>
        public ProductVersion_CompareCheck_Single(IFileVersionExtractor fileVerExtractor, IFileSystem fileSystem, string localProgram, string sourceProgram)
        {
            _fileVerExtractor = fileVerExtractor;
            _fileSystem = fileSystem;
            _localProgram = localProgram;
            _sourceProgram = sourceProgram;
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

            var checkResult = _fileSystem.Exists(_localProgram) && _fileSystem.Exists(_sourceProgram);

            if (!checkResult)
                return new ResultRecord(false, GetType().Name, new string[] { "Could not find " + _localProgram + " or " + _sourceProgram + Extend(TextConstants.POSTFIX_ERROR) });

            if (!_fileVerExtractor.TryGetVersion(out var sourceVersion, _sourceProgram))
                return new ResultRecord(false, GetType().Name, new string[] { "Could not determine version of: " + _sourceProgram + Extend(TextConstants.POSTFIX_ERROR) });

            if (!_fileVerExtractor.TryGetVersion(out var localVersion, _localProgram))
                return new ResultRecord(false, GetType().Name, new string[] { "Could not determine version of:" + TextConstants.POSTFIX_ERROR + _localProgram + Extend(TextConstants.POSTFIX_ERROR) });

            var postfix = TextConstants.POSTFIX_OK;

            if (sourceVersion.CompareTo(localVersion) == 1)
            {
                checkResult = false;
                postfix = TextConstants.POSTFIX_ERROR;
            }

            info.Add(_localProgram + TextConstants.DELIMITER + "version: " + localVersion + Extend(postfix));
            info.Add(_sourceProgram + TextConstants.DELIMITER + "version: " + sourceVersion + Extend(postfix));

            var localFileInfo = _fileSystem.GetFileInfo(_localProgram);
            var sourceFileInfo = _fileSystem.GetFileInfo(_sourceProgram);

            var localModifiedDate = localFileInfo.LastWriteTime;
            var sourceModifiedDate = sourceFileInfo.LastWriteTime;

            if (sourceModifiedDate != localModifiedDate)
                postfix = TextConstants.POSTFIX_WARNING;
            else
                postfix = TextConstants.POSTFIX_OK;

            info.Add(_localProgram + TextConstants.DELIMITER + "last modified time: " + localModifiedDate + Extend(postfix));
            info.Add(_sourceProgram + TextConstants.DELIMITER + "last modified time: " + sourceModifiedDate + Extend(postfix));

            // Compare file sizes in bytes
            long localSize = localFileInfo.Length;
            long sourceSize = sourceFileInfo.Length;

            if (localSize != sourceSize)
                postfix = TextConstants.POSTFIX_WARNING;
            else
                postfix = TextConstants.POSTFIX_OK;

            info.Add(_localProgram + TextConstants.DELIMITER + "file size (bytes): " + localSize + Extend(postfix));
            info.Add(_sourceProgram + TextConstants.DELIMITER + "file size (bytes): " + sourceSize + Extend(postfix));

            return new ResultRecord(checkResult, GetType().Name, info.ToArray());
        }

        private string Extend(string postfix)
        {
            return TextConstants.DELIMITER + postfix;
        }
    }
}
