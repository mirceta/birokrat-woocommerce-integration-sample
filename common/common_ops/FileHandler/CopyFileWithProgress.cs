using System.IO;
using System.Threading.Tasks;

namespace common_ops.FileHandler
{
    public class CopyFileWithProgress : ICopyFileWithProgress
    {
        private long _bytesCopied = 0;
        private readonly object _lock = new object();

        public long BytesCopied => _bytesCopied;

        public async Task Copy(string sourceFilePath, string targetFilePath)
        {
            using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] buffer = new byte[81920];
                int bytesRead;
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await targetStream.WriteAsync(buffer, 0, bytesRead);
                    lock (_lock)
                        _bytesCopied += bytesRead;
                }
            }
        }

        public void Reset()
        {
            _bytesCopied = 0;
        }
    }
}
