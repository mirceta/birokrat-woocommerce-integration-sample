using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops.FileHandler
{
    public interface IDirectoryContentHandler
    {
        Task CopyDirectoryAsync(string sourceDir, string targetDir, bool overwrite = true);
        List<string> DeleteAllContent(string folderPath, params string[] excluded);
        int GetTotalFilesInDirectory(string directoryPath, params string[] filesToExclude);
        string[] GetAllFilesInDirectory(string directoryPath, params string[] filesToExclude);
    }
}
