using System.IO;

namespace common_ops.diagnostics.Checks.Location.Utils
{
    public interface ILocationHelper
    {
        (bool Result, string[] CheckInfo) Check(string[] local, string[] required, string mode);
        (bool Result, string[] CheckInfo) AreAllRequiredFilesPresent(FileInfo[] localFiles, params string[] requiredFiles);
        (bool Result, string[] CheckInfo) AreAllRequiredFoldersPresent(DirectoryInfo[] localFolders, params string[] requiredFolders);
        (string location, bool result) CheckIfFolderExists(string location, string defaultLocation);
        bool IsReadPermissionGranted(string path);
        bool IsWritePermissionGranted(string path);
    }
}
