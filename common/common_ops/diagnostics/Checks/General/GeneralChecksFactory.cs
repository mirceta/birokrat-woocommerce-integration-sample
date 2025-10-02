using common_ops.Abstractions;
using common_ops.diagnostics.Checks.General.Checks;
using common_ops.diagnostics.Checks.General.Utils;
using common_ops.diagnostics.Utils;
using common_ops.FileHandler;
using System;

namespace common_ops.diagnostics.Checks.General
{
    public class GeneralChecksFactory
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
        public BinaryFile_CheckAndMaybeRestore Build_BinaryFile_CheckAndMaybeRestore(
            string localFile_FullName,
            string sourceBinaryFile_FullName,
            bool doRestore = false)
        {
            return new BinaryFile_CheckAndMaybeRestore(new BinaryRestore(), localFile_FullName, sourceBinaryFile_FullName, doRestore);
        }

        /// <summary>
        /// Will compare the file count between source and origin directory. If file count matches result will be true. Class allows to exclude specific names or specifix extensions.
        /// </summary>
        /// <remarks>
        /// the local version. <see cref="ResultRecord.AdditionalInfo"/> contains: file count for origin and source separated with || (<see cref="TextConstants.DELIMITER"/>)
        /// </remarks>
        public FileCountVerifier_Check Build_FileCountVerifier_Check(
            string sourceDirectory,
            string originDirectory,
            params string[] filesToExclude)
        {
            return new FileCountVerifier_Check(
                new DirectoryContentHandlerFactory().Build(null),
                sourceDirectory,
                originDirectory,
                filesToExclude);
        }

        /// <summary>
        /// Checks program version and compare it to source program version. Both paths need to be provided. Will return false
        /// if  sourceVersion is larger than local version. If version match it will also return last modified dates.
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains:
        /// <para>localVersion</para>
        /// <para>sourceVersion</para>
        /// Separated with <c>||</c> in case of an ERROR</para>
        /// </summary>
        public ProductVersion_CompareCheck_Single Build_ProductVersion_CompareCheck_Single(string localProgram, string sourceProgram)
        {
            return new ProductVersion_CompareCheck_Single(
                new FileVersionExtractor(),
                new FileSystem(),
                localProgram,
                sourceProgram);
        }

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
        public ProductVersion_CompareCheck_Multiple Build_ProductVersion_CompareCheck_Multiple(
            Func<string, string, ProductVersion_CompareCheck_Single> simpleVerCompareFactory,
            string localProgramPath,
            string sourceProgramRootPath)
        {
            return new ProductVersion_CompareCheck_Multiple(
                (local, origin) => Build_ProductVersion_CompareCheck_Single(local, origin),
                new DirectorySystem(),
                new FileSystem(),
                localProgramPath,
                sourceProgramRootPath);
        }

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
        public ProductVersion_ExtractLatestVersion Build_ProductVersion_ExtractLatestVersion(string[] files, bool extractOnlyLatest)
        {
            return new ProductVersion_ExtractLatestVersion(new FileVersionExtractor(), files, extractOnlyLatest);
        }
    }
}
