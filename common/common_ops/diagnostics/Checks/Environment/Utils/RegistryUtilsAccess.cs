using si.birokrat.next.common.registration;

namespace common_ops.diagnostics.Checks.Environment.Utils
{
    public class RegistryUtilsAccess : IRegistryUtils
    {
        public bool FixRegistryValue(string registryKey, string key, string newValue)
        {
            return RegistryUtils.FixRegistryValue(registryKey, key, newValue);
        }

        public string GetRegistryValue(string registryKey, string itemKey)
        {
            return RegistryUtils.GetRegistryValue(registryKey, itemKey);
        }
    }
}
