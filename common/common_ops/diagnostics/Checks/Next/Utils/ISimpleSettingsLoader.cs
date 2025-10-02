using System.Collections.Generic;

namespace common_ops.diagnostics.Checks.Next.Utils
{
    public interface ISimpleSettingsLoader
    {
        Dictionary<string, string> LoadSettings(string pathToFile);
        void SaveSettings(string path, Dictionary<string, string> dict);
    }
}
