using System;
using System.IO;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Utils
{
    public class BinaryRestore
    {
        public async Task<bool> RunAsync(string binaryFile, string fileFullName)
        {
            if (!File.Exists(binaryFile))
                binaryFile = $"{Path.GetFileNameWithoutExtension(binaryFile)}.bin";

            if (!File.Exists(binaryFile))
                throw new Exception($"File {binaryFile} does not exist");

            Directory.CreateDirectory(Path.GetDirectoryName(fileFullName));
            var fileName = Path.GetFileNameWithoutExtension(binaryFile);

            using (FileStream sourceStream = new FileStream(binaryFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                // Create or overwrite the destination file
                using (FileStream destinationStream = new FileStream(fileFullName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    // CopyToArchive data from source to destination asynchronously
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
            return true;
        }
    }
}
