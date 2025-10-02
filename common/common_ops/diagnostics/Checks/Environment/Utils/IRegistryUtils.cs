namespace common_ops.diagnostics.Checks.Environment.Utils
{
    public interface IRegistryUtils
    {
        bool FixRegistryValue(string registryKey, string key, string newValue);
        string GetRegistryValue(string registryKey, string itemKey);
    }
}