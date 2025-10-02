using common_ops.Abstractions;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.IO;

namespace common_ops.diagnostics.Checks.Environment.Utils
{
    public class IniSettingsHelper : IIniSettingsHelper
    {
        private readonly IFileSystem _fileSystem;

        public IniSettingsHelper(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Dictionary<string, string> BuildCompareDictionary(string sqlServer)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "SQN", sqlServer },
                { "NEXT", "-1" },
                { "MSGBOX", "0" },
                { "RTCOFF", "0" },
                { "RTC", "-1" },
                { "4USONLY", "-1" },
                { "4USONLY_RTC", "-1" }
            };
        }

        public string[] BuildNotWantedKeysArray()
        {
            return new string[]
            {
                "DS",
            };
        }

        public Dictionary<string, string> GenerateDictionaryFromIni_FixDuplicateValues(string iniLocation, List<string> results = null)
        {
            Dictionary<string, string> iniDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var doSave = false;

            using (FileStream stream = new FileStream(iniLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var content = line.Split('=');

                    if (iniDict.ContainsKey(content[0]))
                    {
                        doSave = true;
                        continue; //duplicate
                    }

                    if (content.Length == 2)
                        iniDict.Add(content[0].Trim(), content[1].Trim());
                    else
                    {
                        var key = string.IsNullOrEmpty(content[0]) ? "NULL" : content[0];
                        var value = string.IsNullOrEmpty(content[1]) ? "NULL" : content[1];

                        if (results != null)
                        {
                            AddCheckToResults(results, new KeyValuePair<string, string>(key, value), TextConstants.POSTFIX_ERROR);
                        }
                    }
                }
            }

            if (doSave)
                SaveIni(iniLocation, iniDict);

            return iniDict;
        }

        public void AddCheckToResults(List<string> results, KeyValuePair<string, string> kvp, string postfix)
        {
            results.Add(kvp.Key + TextConstants.DELIMITER + kvp.Value + TextConstants.DELIMITER + postfix);
        }

        public void SaveIni(string location, Dictionary<string, string> dict)
        {
            var linesToSave = new List<string>();
            foreach (var kvp in dict)
                linesToSave.Add(kvp.Key + "=" + kvp.Value);
            try
            {
                _fileSystem.WriteAllLines(location, linesToSave);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
