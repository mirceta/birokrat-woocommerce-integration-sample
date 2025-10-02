using common_ops.diagnostics.Checks.Environment.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Environment.Checks
{
    /// <summary>
    /// Checks and compares if same values are in Birokrat.ini and registry. Will only return fields with the same name and corresponding value.
    /// if both values are the same output will be OK. Otherwise it will show and ERROR with correcponding value and value origin (registry and ini)
    ///
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: contains result for each field, origin and value with postfix</para>
    /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR</para>
    /// </summary>
    public class Birokrat_RegistryAndIniComparer_Check : ICheck
    {
        private readonly List<string> _fieldsToCheck;
        private readonly string _iniLocation;
        private readonly IIniSettingsHelper _iniSettingsHelper;
        private readonly IRegistrySettingsHelper _registrySettingsHelper;
        private readonly IRegistryUtils _registryUtils;

        /// <summary>
        /// <inheritdoc cref="Birokrat_RegistryAndIniComparer_Check"/>
        /// </summary>
        public Birokrat_RegistryAndIniComparer_Check(
            IIniSettingsHelper iniSettingsHelper,
            IRegistrySettingsHelper registrySettingsHelper,
            IRegistryUtils registryUtils,
            string sqlServerName,
            string biroExeLoc = "")
        {
            var location = string.IsNullOrEmpty(biroExeLoc) ? BiroLocationConstants.BirokratDefaultLocation : biroExeLoc;
            _iniLocation = Path.Combine(location, "Birokrat.ini");
            _iniSettingsHelper = iniSettingsHelper;
            _registrySettingsHelper = registrySettingsHelper;
            _registryUtils = registryUtils;

            _fieldsToCheck = _iniSettingsHelper.BuildCompareDictionary(sqlServerName).Select(x => x.Key).ToList();
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                var results = CompareValues();
                return new ResultRecord(results.All(x => x.Result == true), GetType().Name, results.Select(x => x.Context).ToArray());
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " ERROR");
            }
        }

        private List<(bool Result, string Context)> CompareValues()
        {
            var results = new List<(bool, string)>();

            var iniDict = _iniSettingsHelper.GenerateDictionaryFromIni_FixDuplicateValues(_iniLocation);

            foreach (var item in _fieldsToCheck)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                var compareResult = CompareValuesIfKeyExist(item, iniDict);
                results.AddRange(compareResult);
            }

            if (!results.Any())
                throw new Exception("Ini or registry values missing! " + TextConstants.POSTFIX_WARNING);

            return results;
        }

        private List<(bool Result, string Context)> CompareValuesIfKeyExist(string key, Dictionary<string, string> iniDict)
        {
            var info = new List<(bool, string)>();

            var registryValue = _registryUtils.GetRegistryValue(_registrySettingsHelper.REGISTRY_KEY, key);

            if (registryValue == null)
                return info;

            if (!iniDict.TryGetValue(key, out var iniValue))
                return info;

            if (iniValue.Trim().Equals(registryValue.Trim(), StringComparison.OrdinalIgnoreCase))
                info.Add((true, BuildInfoLog(key, iniValue, "RegistryAndIni", TextConstants.POSTFIX_OK)));
            else
            {
                info.Add((false, BuildInfoLog(key, iniValue, "Ini", TextConstants.POSTFIX_ERROR)));
                info.Add((false, BuildInfoLog(key, registryValue, "Registry", TextConstants.POSTFIX_ERROR)));
            }
            return info;
        }

        private static string BuildInfoLog(string key, string value, string origin, string postfix)
        {
            return key + TextConstants.DELIMITER + origin + TextConstants.DELIMITER + value + TextConstants.DELIMITER + postfix;
        }
    }
}
