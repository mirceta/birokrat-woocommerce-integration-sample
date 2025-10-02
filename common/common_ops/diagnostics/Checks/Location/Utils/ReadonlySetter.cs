using common_ops.Abstractions;
using System.IO;

namespace common_ops.diagnostics.Checks.Location.Utils
{
    public class ReadonlySetter : IReadonlySetter
    {
        private readonly IFileSystem _fileSystem;

        public ReadonlySetter(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void MakeFileReadonly(string file)
        {
            if (_fileSystem.Exists(file))
            {
                var attributes = File.GetAttributes(file);

                if (!attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    File.SetAttributes(file, attributes | FileAttributes.ReadOnly);
                }
            }
        }

        public void RemoveReadonlyFromFile(string file)
        {
            if (_fileSystem.Exists(file))
            {
                File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);
            }
        }
    }
}
