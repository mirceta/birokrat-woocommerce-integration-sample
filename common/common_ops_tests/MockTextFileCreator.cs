using System;
using System.IO;

namespace common_ops_tests
{
    internal class MockTextFileCreator
    {
        private readonly string _path;
        internal readonly FileInfo FileInfo;

        public MockTextFileCreator(string path, bool modifyLastWriteTime = false)
        {
            _path = path;

            if (!File.Exists(_path))
                File.WriteAllText(_path, string.Empty);

            if (modifyLastWriteTime)
                File.SetLastWriteTime(_path, new DateTime(2023, 1, 1, 12, 0, 0));

            FileInfo = new FileInfo(path);
        }

        public void Dispose()
        {
            File.Delete(_path);
        }
    }
}
