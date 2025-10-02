using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Next.Utils;
using common_ops.diagnostics.Constants;
using common_ops.Executors.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Next.Checks
{
    public class BiroNext_Settings_CheckAndRepair : ICheck
    {
        readonly string NEXT_SETTINGS_DEFAULT_SQLSERVER_VALUE = "localhost";

        private readonly string[] SECRETS_TO_CHANGE = new string[]
        {
            "proxy_global",
            "identity_server"
        };

        readonly string[] NEXT_SETTINGS_KEYS_TO_CHECK =
        {
            "LoggingSqlServer",
            "Server",
            "SqlServer",
        };

        readonly string[] NEXT_SETTINGS_DEFAULT_KEYS_TO_CHECK =
        {
            "LoggingSqlServer",
            "Server",
            "SqlServer",
            "Host",
        };

        readonly int NO_OF_APPSETTINGS_REQUIRED = 6;
        readonly int NO_OF_APPSETTINGS_SECRETS_REQUIRED = 5;

        private readonly List<string> _results = new List<string>();
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly ICheck _bironextLocationCheck;
        private readonly IDirectorySystem _directorySystem;
        private readonly ISqlUtils _sqlUtils;
        private readonly IJsonParser _jsonParser;
        private readonly string _rootFolder;
        private readonly string _sqlServerName;
        private readonly string _credentialsSqlServerName;
        private readonly bool _doRepair;

        /// <summary>
        /// Checks and optionally repairs the settings and secrets _files in the specified BiroNext root folder.
        /// Validates whether required appsettings and secrets _files exist and ensures their configurations match the expected SQL Server name.
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains:
        /// Validation results for settings _files (e.g., missing _files, incorrect configurations, repair logs...).
        /// Validation results for secrets _files (e.g., missing _files, incorrect configurations, repair logs...).
        /// If everything is good all entries will be ending wih 'OK'.</para>
        ///
        /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, WARNING, REPAIR, ERROR </para>
        /// 
        /// <para>Will return false if any of the following is true:
        /// 1. The specified root folder is not a valid BiroNext location.
        /// 2. Required settings or secrets _files are missing.
        /// 3. The repair option is not enabled AND Configurations do not match the expected values.</para>
        /// </summary>
        public BiroNext_Settings_CheckAndRepair(
            ICheck bironextLocationCheck,
            IDirectorySystem directorySystem,
            ISqlUtils sqlUtils,
            IJsonParser jsonParser,
            string nextRootFolder,
            string sqlServerName,
            string credentialsSqlServerName,
            bool doRepair = false)
        {
            _bironextLocationCheck = bironextLocationCheck;
            _directorySystem = directorySystem;
            _sqlUtils = sqlUtils;
            _jsonParser = jsonParser;
            _rootFolder = nextRootFolder;
            //IMPORTANT! sqlServer in individual settings should have full actual name of a server like LAPTOP-G2\SQLEXPRESS not localhost\SQLEXPRESS.
            //Otherwise localhost can be mapped wrong sometimes

            _sqlServerName = _sqlUtils.ParseSqlServerToRealName(sqlServerName);
            _credentialsSqlServerName = credentialsSqlServerName;
            _doRepair = doRepair;
        }

        /// <summary>
        /// <inheritdoc cref="BiroNext_Settings_CheckAndRepair"/>
        /// </summary>
        public async Task<ResultRecord> Run()
        {
            try
            {
                return await Work();
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "FATAL ERROR: " + ex);
            }

        }

        private async Task<ResultRecord> Work()
        {
            var safetychekResult = await _bironextLocationCheck.Run();

            if (!safetychekResult.Result)
                return new ResultRecord(false, GetType().Name, "Not valid BiroNext location. Can't check settings");

            var secretsResult = CheckSecrets(_doRepair);
            var settingsResult = CheckSettings(_doRepair);

            return new ResultRecord(secretsResult && settingsResult, GetType().Name, _results.ToArray());
        }

        private bool CheckSettings(bool repair)
        {
            var check = true;

            var appsettings = BiroNextConstants.NextSettingsFileName;
            var settings = _directorySystem.GetFiles(_rootFolder, appsettings, SearchOption.AllDirectories);

            if (settings.Length < NO_OF_APPSETTINGS_REQUIRED)
                throw new Exception(SettingsMissingMessage(appsettings) + " One '" + appsettings + "' should also be in " + _rootFolder + "");

            foreach (var settingsPath in settings)
            {
                Dictionary<string, string> loadedConfig = _jsonParser.BuildConfig(settingsPath);

                if (settingsPath == Path.Combine(_rootFolder, appsettings))
                {
                    var key = "LoggingSqlServer";
                    if (IsKeyPresentAndIsValid(loadedConfig, key, _sqlServerName))
                        _results.Add(OperationOKMessage(settingsPath));
                    else
                    {
                        if (repair)
                        {
                            _results.Add(SettingMessageBeforeRepair(settingsPath, key, loadedConfig[key]));
                            loadedConfig[key] = _sqlServerName;
                            _results.Add(SettingMessageAfterRepair(_sqlServerName));
                            _results.Add(OperationOKMessage(settingsPath));
                        }
                        else
                        {
                            _results.Add(WrongSettingsValuesMessage(settingsPath, key, loadedConfig[key], _sqlServerName));
                            _results.Add(CantRepairSettingsMessage());
                            check = false;
                        }
                    }
                }
                else
                {
                    check = CheckIfOtherConfigsAreDefault(settingsPath, repair, ref loadedConfig);
                }
                var saveResult = _jsonParser.SaveConfig(loadedConfig, settingsPath);
                if (!string.IsNullOrEmpty(saveResult))
                    _results.Add(saveResult);
            }
            return check;
        }

        private bool IsKeyPresentAndIsValid(Dictionary<string, string> loadedConfig, string key, string sqlServer)
        {
            if (loadedConfig.TryGetValue(key, out var value))
            {
                return value.Equals(sqlServer, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private bool CheckSecrets(bool repair)
        {
            var check = true;
            var appsettingsSecrets = BiroNextConstants.NextSettingsSecretsFileName;
            var secrets = _directorySystem.GetFiles(_rootFolder, appsettingsSecrets, SearchOption.AllDirectories);

            if (secrets.Length < NO_OF_APPSETTINGS_SECRETS_REQUIRED)
                throw new Exception(SettingsMissingMessage(appsettingsSecrets));

            foreach (var settingsPath in secrets)
            {
                var config = _jsonParser.BuildConfig(settingsPath);
                if (AreRequiredSecretsSettings(settingsPath, SECRETS_TO_CHANGE))
                {
                    if (CheckSecretsConfig(settingsPath, repair, ref config))
                        _results.Add(OperationOKMessage(settingsPath));
                    else
                        check = false;
                }
                else
                {
                    check = CheckIfOtherConfigsAreDefault(settingsPath, repair, ref config);
                }
                _jsonParser.SaveConfig(config, settingsPath);
            }
            return check;
        }

        private bool CheckSecretsConfig(string settingsPath, bool repair, ref Dictionary<string, string> config)
        {
            var check = true;
            var repairedConfig = new Dictionary<string, string>(config);
            foreach (var field in config)
            {
                foreach (var key in NEXT_SETTINGS_KEYS_TO_CHECK)
                {
                    if (field.Key.EndsWith(key))
                    {
                        var sqlServer = DetermineSqlServerForKey(field);
                        if (string.IsNullOrWhiteSpace(_sqlServerName))
                            throw new ArgumentException("Sql Server Name resolved to empty. Provide a valid SQL Server.", nameof(sqlServer));

                        if (!field.Value.Equals(sqlServer, StringComparison.OrdinalIgnoreCase))
                        {
                            if (repair)
                            {
                                _results.Add(SettingMessageBeforeRepair(settingsPath, field.Key, field.Value));
                                repairedConfig[field.Key] = sqlServer;
                                _results.Add(SettingMessageAfterRepair(sqlServer));
                            }
                            else
                            {
                                _results.Add(WrongSettingsValuesMessage(settingsPath, field.Key, field.Value, sqlServer));
                                _results.Add(CantRepairSettingsMessage());
                                check = false;
                            }
                        }
                        break;
                    }
                }
            }
            config = repairedConfig;
            return check;
        }

        private bool AreRequiredSecretsSettings(string settingsPath, params string[] settings)
        {
            return settings.Any(x => settingsPath.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private string DetermineSqlServerForKey(KeyValuePair<string, string> field)
        {
            var isCredentials = field.Key.IndexOf("Application", StringComparison.OrdinalIgnoreCase) >= 0
                || field.Key.IndexOf("Configuration", StringComparison.OrdinalIgnoreCase) >= 0;

            return isCredentials ? _credentialsSqlServerName : _sqlServerName;
        }

        private bool CheckIfOtherConfigsAreDefault(string settingsPath, bool repair, ref Dictionary<string, string> config)
        {
            bool check = true;
            var repairedConfig = new Dictionary<string, string>(config);
            foreach (var field in config)
            {
                foreach (var key in NEXT_SETTINGS_DEFAULT_KEYS_TO_CHECK)
                {
                    if (field.Key.EndsWith(key))
                    {
                        if (!field.Value.Equals(NEXT_SETTINGS_DEFAULT_SQLSERVER_VALUE, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (repair)
                            {
                                repairedConfig[field.Key] = NEXT_SETTINGS_DEFAULT_SQLSERVER_VALUE;
                                _results.Add(SettingMessageBeforeRepair(settingsPath, field.Key, field.Value));
                                _results.Add(SettingMessageAfterRepair(NEXT_SETTINGS_DEFAULT_SQLSERVER_VALUE));
                            }
                            else
                            {
                                _results.Add(WrongSettingsValuesMessage(settingsPath, field.Key, field.Value, NEXT_SETTINGS_DEFAULT_SQLSERVER_VALUE));
                                check = false;
                            }
                        }
                        break;
                    }
                }
            }
            config = repairedConfig;
            return check;
        }

        #region MESSAGES

        private string OperationOKMessage(string settingsPath)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append("'" + settingsPath);
            _stringBuilder.Append("' " + TextConstants.POSTFIX_OK);

            return _stringBuilder.ToString();
        }

        private string CantRepairSettingsMessage()
        {
            _stringBuilder.Clear();
            _stringBuilder.Append("Repair option is set to false. Repair manually or rerun with repair option");
            _stringBuilder.Append(" " + TextConstants.POSTFIX_WARNING);

            return _stringBuilder.ToString();
        }

        private string SettingMessageBeforeRepair(string settingsPath, string key, string value)
        {
            _stringBuilder.Clear();
            _stringBuilder.AppendLine("'" + settingsPath + "'");
            _stringBuilder.AppendLine("\tWrong setting in field: " + key);
            _stringBuilder.Append("\tValue: " + value);

            return _stringBuilder.ToString();
        }

        private string SettingMessageAfterRepair(string changedValue)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append("Value changed to: ");
            _stringBuilder.Append(changedValue);
            _stringBuilder.Append(" " + TextConstants.POSTFIX_REPAIR);

            return _stringBuilder.ToString();
        }

        private string SettingsMissingMessage(string settingName)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append("At least 1 setting with name '" + settingName);
            _stringBuilder.Append("' missing. Each folder in '" + _rootFolder);
            _stringBuilder.Append("' should have its own '" + settingName);
            _stringBuilder.Append("'.");

            return _stringBuilder.ToString();
        }
        private string WrongSettingsValuesMessage(string settingsPath, string key, string value, string sqlServer)
        {
            _stringBuilder.Clear();
            _stringBuilder.AppendLine("'" + settingsPath + "'");
            _stringBuilder.AppendLine("\tWrong setting in field: " + key);
            _stringBuilder.AppendLine("\tValue: " + value);
            _stringBuilder.Append("\tExpected value: " + sqlServer);

            return _stringBuilder.ToString();
        }
        #endregion
    }
}
