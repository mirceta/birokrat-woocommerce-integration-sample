using System.Collections.Generic;

namespace common_ops.diagnostics.Checks.Environment.Utils
{
    public class RegistrySettingsHelper : IRegistrySettingsHelper
    {
        private readonly string REG_KEY = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Andersen\Birokrat";

        public string REGISTRY_KEY => REG_KEY;

        public Dictionary<string, string> BuildCompareDictionary(string biroExeLocation, string sqlServerName)
        {
            return new Dictionary<string, string>()
            {
                { "Pot", biroExeLocation },
                { "SQLServer", sqlServerName },
                { "4UsOnly", "-1" }
            };
        }
    }
}
