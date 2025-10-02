using common_ops.diagnostics.Constants;
using common_ops.diagnostics.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.General.Checks
{
    /// <summary>
    /// Represents a diagnostic check that ensures the existence of a specified file.
    /// If the file does not exist and the repair option is enabled, it attempts to restore the file 
    /// from a provided binary backup file.
    /// </summary>
    /// <remarks>
    /// This class supports the following result postfixes in <see cref="ResultRecord.AdditionalInfo"/>:
    /// <list type="bullet">
    /// <item><term>OK</term> - The file exists and no further action was required.</item>
    /// <item><term>ERROR</term> - The file does not exist and the repair option is not enabled, or the restoration failed.</item>
    /// <item><term>REPAIR</term> - The file was successfully restored from the binary backup.</item>
    /// </list>
    /// </remarks>
    public class BinaryFile_CheckAndMaybeRestore : ICheck
    {
        private readonly string _sourceBinaryFile_FullName;
        private readonly BinaryRestore _binaryRestore;
        private readonly string _localFile_FullName;
        private readonly bool _doRestore;

        /// <summary>
        /// <inheritdoc cref="BinaryFile_CheckAndMaybeRestore"/>
        /// </summary>
        public BinaryFile_CheckAndMaybeRestore(
            BinaryRestore binaryRestore,
            string localFile_FullName,
            string sourceBinaryFile_FullName,
            bool doRestore = false)
        {
            _sourceBinaryFile_FullName = sourceBinaryFile_FullName;
            _binaryRestore = binaryRestore;
            _localFile_FullName = localFile_FullName;
            _doRestore = doRestore;
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
            if (File.Exists(_localFile_FullName))
                return new ResultRecord(true, GetType().Name, _localFile_FullName + " was found. " + TextConstants.POSTFIX_OK);

            if (!_doRestore)
                return new ResultRecord(false, GetType().Name, _localFile_FullName + " was not found. Repair option not selected! " + TextConstants.POSTFIX_ERROR);

            var result = await _binaryRestore.RunAsync(_sourceBinaryFile_FullName, _localFile_FullName);
            var message = result ? _localFile_FullName + " restored. " + TextConstants.POSTFIX_REPAIR : _localFile_FullName + " restoration failed. " + TextConstants.POSTFIX_ERROR;

            return new ResultRecord(result, GetType().Name, message);
        }
    }
}
