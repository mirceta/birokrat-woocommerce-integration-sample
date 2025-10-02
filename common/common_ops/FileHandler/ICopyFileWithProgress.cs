using System.Threading.Tasks;

namespace common_ops.FileHandler
{
    public interface ICopyFileWithProgress
    {
        long BytesCopied { get; }

        Task Copy(string sourceFilePath, string targetFilePath);
        void Reset();
    }
}