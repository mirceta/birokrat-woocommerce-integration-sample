using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Location.Utils;
using System;

namespace common_ops.FileHandler
{
    public class DirectoryContentHandlerFactory
    {
        public IDirectoryContentHandler Build(Action<string> logger, int timeInMills = 5000)
        {
            return new DirectoryContentHandler(
                new FileSystem(),
                new DirectorySystem(),
                new PathSystem(),
                new CopyFileWithProgress(),
                logger,
                timeInMills);
        }
    }
}
