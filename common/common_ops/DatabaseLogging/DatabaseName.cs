using System;

namespace common_ops.DatabaseLogging
{
    internal class DatabaseName
    {
        private readonly string _baseName;

        public DatabaseName(string baseName)
        {
            _baseName = baseName;
        }

        internal string BaseName => _baseName;

        internal string Name => $"{_baseName}_{DateTime.Now.Year}_{DateTime.Now.Month}";
    }
}
