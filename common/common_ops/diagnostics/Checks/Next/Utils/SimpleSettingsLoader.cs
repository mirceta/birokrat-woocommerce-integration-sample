using common_ops.Abstractions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace common_ops.diagnostics.Checks.Next.Utils
{
    public class SimpleSettingsLoader : ISimpleSettingsLoader
    {
        private readonly IFileSystem _fileSystem;

        public SimpleSettingsLoader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Dictionary<string, string> LoadSettings(string pathToFile)
        {
            if (!_fileSystem.Exists(pathToFile))
                throw new FileNotFoundException($"Settings '{pathToFile}' not found");

            var json = _fileSystem.ReadAllText(pathToFile);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public void SaveSettings(string path, Dictionary<string, string> dict)
        {
            var json = JsonConvert.SerializeObject(dict);
            File.WriteAllText(path, json);
        }
    }
}
