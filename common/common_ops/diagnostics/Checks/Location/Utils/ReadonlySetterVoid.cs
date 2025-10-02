using common_ops.Abstractions;
using System.IO;

namespace common_ops.diagnostics.Checks.Location.Utils
{
    public class ReadonlySetterVoid : IReadonlySetter
    {
        private readonly IFileSystem _fileSystem;

        public ReadonlySetterVoid(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void MakeFileReadonly(string file)
        {

        }

        public void RemoveReadonlyFromFile(string file)
        {

        }
    }
}
