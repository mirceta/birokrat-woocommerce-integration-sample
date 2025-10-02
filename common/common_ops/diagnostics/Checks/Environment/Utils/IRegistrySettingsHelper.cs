using System.Collections.Generic;

namespace common_ops.diagnostics.Checks.Environment.Utils
{
    public interface IRegistrySettingsHelper
    {
        string REGISTRY_KEY { get; }
        Dictionary<string, string> BuildCompareDictionary(string biroExeLocation, string sqlServerName);
    }
}
