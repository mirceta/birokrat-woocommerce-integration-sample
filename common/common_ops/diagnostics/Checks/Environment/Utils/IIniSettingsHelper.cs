using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Environment.Utils
{
    public interface IIniSettingsHelper
    {
        void AddCheckToResults(List<string> results, KeyValuePair<string, string> kvp, string postfix);
        Dictionary<string, string> BuildCompareDictionary(string sqlServer);
        string[] BuildNotWantedKeysArray();
        Dictionary<string, string> GenerateDictionaryFromIni_FixDuplicateValues(string iniLocation, List<string> results = null);
        void SaveIni(string location, Dictionary<string, string> repairedDict);
    }
}
