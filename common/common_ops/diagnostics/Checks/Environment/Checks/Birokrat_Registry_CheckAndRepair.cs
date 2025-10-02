using common_ops.diagnostics.Checks.Environment.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Environment.Checks
{
    /// <summary>
    /// Checks and optionally repairs registry values related to the Birokrat application.
    /// 
    /// <para>The class verifies registry values against expected values stored in a dictionary. If the `doRepair` flag is enabled, 
    /// incorrect registry values will be fixed automatically. Repair will also add record to registry if it is missing</para>
    ///
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: contains result for each registry record with postfix</para>
    /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR, REPAIR</para>
    /// </summary>
    public class Birokrat_Registry_CheckAndRepair : ICheck
    {
        private readonly string _sqlServerName;
        private readonly Dictionary<string, string> _checks;
        private readonly IRegistrySettingsHelper _registrySettingsHelper;
        private readonly IRegistryUtils _registryUtils;
        private readonly bool _repair;
        private readonly Func<string, bool> _waitForConfirmation;

        private readonly StringBuilder _sb = new StringBuilder();

        /// <summary>
        /// <inheritdoc cref="Birokrat_Registry_CheckAndRepair"/>
        /// </summary>
        public Birokrat_Registry_CheckAndRepair(
            IRegistrySettingsHelper registrySettingsHelper,
            IRegistryUtils registryUtils,

            string sqlServerName,
            string biroExeLoc = "",
            bool doRepair = false,
            Func<string, bool> waitForConfirmation = null)
        {
            biroExeLoc = string.IsNullOrEmpty(biroExeLoc) ? BiroLocationConstants.BirokratDefaultLocation : biroExeLoc;
            _registrySettingsHelper = registrySettingsHelper;
            _registryUtils = registryUtils;
            _repair = doRepair;
            _waitForConfirmation = waitForConfirmation;

            _checks = _registrySettingsHelper.BuildCompareDictionary(biroExeLoc, sqlServerName);
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                var result = Work();
                return result;
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " ERROR");
            }
        }

        private ResultRecord Work()
        {
            var results = new List<(bool Result, string Context)>();

            if (_checks.Count == 0)
                throw new Exception("No values in registry!");

            foreach (var item in _checks)
            {
                if (string.IsNullOrEmpty(item.Value))
                {
                    results.Add((false, BuildWrongRegValueMessage(item.Key, item.Value)));
                    continue;
                }

                var result = _registryUtils.GetRegistryValue(_registrySettingsHelper.REGISTRY_KEY, item.Key);

                if (result == null)
                {
                    if (FixRegistryRecord(_registrySettingsHelper.REGISTRY_KEY, item.Key, item.Value, string.Empty))
                    {
                        _sb.Clear();
                        _sb.Append("Registry record added! Key: '" + item.Key + "'");
                        _sb.Append(TextConstants.DELIMITER);
                        _sb.Append("Value: '" + item.Value + "'");
                        _sb.Append(TextConstants.DELIMITER + TextConstants.POSTFIX_REPAIR);
                        results.Add((true, _sb.ToString()));
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(result))
                    result = "NULL";

                if (result.Equals(item.Value, StringComparison.CurrentCultureIgnoreCase))
                {
                    _sb.Clear();
                    _sb.Append("Registry key found: '" + item.Key + "'");
                    _sb.Append(TextConstants.DELIMITER);
                    _sb.Append("'Value: '" + item.Value + "'");
                    _sb.Append(TextConstants.DELIMITER + TextConstants.POSTFIX_OK);

                    results.Add((true, _sb.ToString()));
                }
                else
                {
                    if (FixRegistryRecord(_registrySettingsHelper.REGISTRY_KEY, item.Key, item.Value, result))
                    {
                        _sb.Clear();
                        _sb.Append("Registry value repaired! Key: '" + item.Key + "'");
                        _sb.Append(TextConstants.DELIMITER);
                        _sb.Append("'Value: '" + item.Value + "'");
                        _sb.Append(TextConstants.DELIMITER);
                        _sb.Append("'Old value: '" + result + "'");
                        _sb.Append(TextConstants.DELIMITER + TextConstants.POSTFIX_REPAIR);

                        results.Add((true, _sb.ToString()));
                        continue;
                    }
                    results.Add((false, BuildWrongRegValueMessage(item.Key, item.Value)));
                }
            }

            return new ResultRecord(results.All(x => x.Result == true), GetType().Name, results.Select(x => x.Context).ToArray());
        }

        private string BuildWrongRegValueMessage(string key, string value)
        {
            _sb.Clear();
            _sb.Append("Registry value or key is wrong! Key: '" + key + TextConstants.DELIMITER);
            _sb.Append("Value: '" + value + "'" + TextConstants.DELIMITER);
            _sb.Append(TextConstants.POSTFIX_ERROR);
            return _sb.ToString();
        }

        private bool FixRegistryRecord(string registryKey, string key, string newValue, string oldValue)
        {
            if (_repair && _waitForConfirmation != null)
            {
                if (!_waitForConfirmation.Invoke(BuildConfirmationWindowMessage(key, newValue, oldValue)))
                    return false;

                if (_registryUtils.FixRegistryValue(registryKey, key, newValue))
                    return true;
            }
            return false;
        }

        private string BuildConfirmationWindowMessage(string key, string newValue, string oldValue)
        {
            if (string.IsNullOrEmpty(oldValue))
                return "Registry key '" + key + "' not found!" + System.Environment.NewLine + "Create key with Value: '" + newValue + "'?";
            return "Fix registry key '" + key + "', Value: '" + oldValue + "'." + System.Environment.NewLine + "Replace with new Value: '" + newValue + "'?";
        }
    }
}
