using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Environment.Utils;
using common_ops.diagnostics.Checks.Location.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Environment.Checks
{
    /// <summary>
    /// Will check if ini settings for 00000000 are filled correctly. Checks fields SQL, NEXT, Msgbox and RTC. If repair option is set
    /// it will fix or add fields with REPAIR postfix.
    /// Will not check customers inis since they will be created/updated automatically by birokrat
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: All checked .ini field, value of the fields and postfixes</para>
    /// 
    /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR, REPAIR</para>
    /// </summary>
    public class Birokrat_IniSettings_CheckAndRepair : ICheck
    {
        private readonly Dictionary<string, string> INI_FIELDS_WITH_VALUES;
        private readonly string[] KEYS_TO_REMOVE;

        private readonly string _location;
        private readonly IIniSettingsHelper _iniSettingsHelper;
        private readonly IReadonlySetter _readonlySetter;
        private readonly IFileSystem _fileSystem;
        private readonly bool _repair;

        /// <summary>
        /// <inheritdoc cref="Birokrat_IniSettings_CheckAndRepair"/>
        /// </summary>
        public Birokrat_IniSettings_CheckAndRepair(
            IIniSettingsHelper iniSettingsHelper,
            IReadonlySetter readonlySetter,
            IFileSystem fileSystem,
            string sqlServerName,
            string location = "",
            bool doRepair = false)
        {
            location = string.IsNullOrEmpty(location) ? BiroLocationConstants.BirokratDefaultLocation : location;

            _location = Path.Combine(location, "Birokrat.ini");
            _iniSettingsHelper = iniSettingsHelper;
            _readonlySetter = readonlySetter;
            _fileSystem = fileSystem;
            _repair = doRepair;

            INI_FIELDS_WITH_VALUES = _iniSettingsHelper.BuildCompareDictionary(sqlServerName);
            KEYS_TO_REMOVE = _iniSettingsHelper.BuildNotWantedKeysArray();
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                return await Work();
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
            finally
            {
                _readonlySetter.MakeFileReadonly(_location);
            }
        }

        private async Task<ResultRecord> Work()
        {
            if (IsIniFileEmptyOrMissing(_location))
            {
                _iniSettingsHelper.SaveIni(_location, INI_FIELDS_WITH_VALUES);
                _readonlySetter.MakeFileReadonly(_location);
                return new ResultRecord(true, GetType().Name, ".ini file regenerated" + TextConstants.DELIMITER + TextConstants.DELIMITER + TextConstants.POSTFIX_REPAIR);
            }

            var results = new List<string>();
            var checkResult = true;

            var doSave = false;
            Dictionary<string, string> iniDict = _iniSettingsHelper.GenerateDictionaryFromIni_FixDuplicateValues(_location, results);
            Dictionary<string, string> repairedDict = _iniSettingsHelper.GenerateDictionaryFromIni_FixDuplicateValues(_location, results);

            foreach (var kvp in INI_FIELDS_WITH_VALUES)
            {
                if (iniDict.ContainsKey(kvp.Key))
                {
                    if (iniDict[kvp.Key].Equals(kvp.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        _iniSettingsHelper.AddCheckToResults(results, kvp, TextConstants.POSTFIX_OK);
                    }
                    else //if value is not the same
                    {
                        if (_repair)
                        {
                            repairedDict[kvp.Key] = kvp.Value;
                            doSave = true;
                            _iniSettingsHelper.AddCheckToResults(results, kvp, TextConstants.POSTFIX_REPAIR);
                        }
                        else
                        {
                            _iniSettingsHelper.AddCheckToResults(results, kvp, TextConstants.POSTFIX_ERROR);
                            checkResult = false;
                        }
                    }
                }
                else //if key is not present
                {
                    if (_repair)
                    {
                        repairedDict.Add(kvp.Key, kvp.Value);
                        doSave = true;
                        _iniSettingsHelper.AddCheckToResults(results, kvp, TextConstants.POSTFIX_REPAIR);
                    }
                    else
                    {
                        _iniSettingsHelper.AddCheckToResults(results, kvp, TextConstants.POSTFIX_ERROR);
                        checkResult = false;
                    }
                }
            }

            if (_repair) // Here we remove unnecessary or faulty keys
            {
                var length = repairedDict.Count;
                repairedDict = repairedDict.Where(x => !KEYS_TO_REMOVE.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                if (length > repairedDict.Count)
                    doSave = true;
            }

            if (doSave && _repair)
            {
                _readonlySetter.RemoveReadonlyFromFile(_location);
                _iniSettingsHelper.SaveIni(_location, repairedDict);
                await Task.Delay(200);
            }

            return new ResultRecord(checkResult, GetType().Name, results.ToArray());
        }

        private bool IsIniFileEmptyOrMissing(string location)
        {
            if (!_fileSystem.Exists(_location))
                return true;
            var content = _fileSystem.ReadAllText(_location).Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
            if (string.IsNullOrEmpty(content))
                return true;
            return false;
        }
    }
}
