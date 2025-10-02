using System.Collections.Generic;

namespace common_ops.diagnostics.Checks.Next.Utils
{
    public interface IJsonParser
    {
        Dictionary<string, string> BuildConfig(string item);
        Dictionary<string, string> FlattenJson(string json);
        string SaveConfig(Dictionary<string, string> config, string filePath);
        string UnflattenJson(Dictionary<string, string> flatConfig);
    }
}
