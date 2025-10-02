using common_ops.Abstractions;
using common_ops.diagnostics;
using common_ops.diagnostics.Checks.Next.Checks;
using common_ops.diagnostics.Checks.Next.Utils;
using common_ops.diagnostics.Constants;
using common_ops.Executors.Sql;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.NextChecksTests
{
    [TestFixture]
    public class BiroNext_Settings_CheckAndRepair_Tests
    {
        Mock<ICheck> _bironextLocationCheckMock;
        Mock<IDirectorySystem> _directorySystemMock;
        Mock<ISqlUtils> _sqlUtilsMock;
        Mock<IJsonParser> _jsonParserMock;

        private string _sqlServerName;
        private string _credentialSqlServerName;
        private string _nextRootFolder;

        private List<string> _secretsLocations;
        private List<string> _appSettingsLocations;
        private Dictionary<string, string> _correctSecrets;
        private Dictionary<string, string> _correctDefaultSecrets;
        private Dictionary<string, string> _correctAppsettings;
        private Dictionary<string, string> _correctDefaultAppsettings;

        [SetUp]
        public void SetUp()
        {
            _bironextLocationCheckMock = new Mock<ICheck>();
            _directorySystemMock = new Mock<IDirectorySystem>();
            _jsonParserMock = new Mock<IJsonParser>();
            _sqlUtilsMock = new Mock<ISqlUtils>();

            _jsonParserMock.Setup(x => x.SaveConfig(It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()))
                .Callback(() => { });

            _sqlServerName = "SQLEXPRESS\\nekineki";
            _credentialSqlServerName = "CREDENTIALS\\SKEWL";
            _nextRootFolder = "c:\\next\\Location";

            _secretsLocations = new List<string>
            {
                $"{_nextRootFolder}\\api_core\\appsettings.Secrets.json",
                $"{_nextRootFolder}\\api_wrapper\\appsettings.Secrets.json",
                $"{_nextRootFolder}\\biro_instance_pool\\appsettings.Secrets.json",
                $"{_nextRootFolder}\\identity_server\\appsettings.Secrets.json",
                $"{_nextRootFolder}\\proxy_global\\appsettings.Secrets.json"
            };

            _appSettingsLocations = new List<string>
            {
                $"{_nextRootFolder}\\api_core\\appsettings.json",
                $"{_nextRootFolder}\\api_wrapper\\appsettings.json",
                $"{_nextRootFolder}\\biro_instance_pool\\appsettings.json",
                $"{_nextRootFolder}\\identity_server\\appsettings.json",
                $"{_nextRootFolder}\\proxy_global\\appsettings.json",
                $"{_nextRootFolder}\\appsettings.json"
            };

            _correctSecrets = new Dictionary<string, string>()
            {
                { "ApplicationDb:Server", $"{_credentialSqlServerName}" },
                { "ApplicationDb:Database", "application" },
                { "ApplicationDb:TrustedConnection", "True" },
                { "SqlServer", $"{_sqlServerName}" },
                { "Email:Host", "smtp.gmail.com" }
            };

            _correctDefaultSecrets = new Dictionary<string, string>()
            {
                { "ApplicationDb:Server", "localhost" },
                { "ApplicationDb:Database", "application" },
                { "ApplicationDb:TrustedConnection", "True" },
                { "SqlServer", "localhost" }
            };

            _correctAppsettings = new Dictionary<string, string>()
            {
                { "LoggingSqlServer", $"{_sqlServerName}" }
            };

            _correctDefaultAppsettings = new Dictionary<string, string>()
            {
                { "LoggingSqlServer", "localhost" }
            };
        }

        private BiroNext_Settings_CheckAndRepair BuildCheck(bool doRepair)
        {
            return new BiroNext_Settings_CheckAndRepair(
                _bironextLocationCheckMock.Object,
                _directorySystemMock.Object,
                _sqlUtilsMock.Object,
                _jsonParserMock.Object,
                _nextRootFolder,
                _sqlServerName,
                _credentialSqlServerName,
                doRepair);
        }

        [Test]
        public async Task Run_AllSettingsAreOK_ReturnsSuccess()
        {
            _bironextLocationCheckMock.Setup(x => x.Run())
                .ReturnsAsync(new ResultRecord { Result = true });

            _sqlUtilsMock.Setup(x => x.ParseSqlServerToRealName(It.IsAny<string>()))
                .Returns(_sqlServerName);

            _directorySystemMock.SetupSequence(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(_secretsLocations.ToArray())
                .Returns(_appSettingsLocations.ToArray());

            _jsonParserMock.Setup(x => x.BuildConfig(It.IsAny<string>()))
                .Returns((string input) =>
                {
                    if (input.EndsWith(BiroNextConstants.NextSettingsSecretsFileName)) //appsettings.Secrets
                    {
                        if (input.Contains("identity_server", StringComparison.OrdinalIgnoreCase))
                            return _correctSecrets;
                        if (input.Contains("proxy_global", StringComparison.OrdinalIgnoreCase))
                            return _correctSecrets;
                        return _correctDefaultSecrets;
                    }
                    else //appsettings
                    {
                        if (input.Contains(Path.Combine(_nextRootFolder, BiroNextConstants.NextSettingsFileName), StringComparison.OrdinalIgnoreCase))
                            return _correctAppsettings;
                        return _correctDefaultAppsettings;
                    }
                });

            var result = await BuildCheck(false).Run();
            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo.All(x => x.Contains(TextConstants.POSTFIX_OK)), Is.True);
        }

        [Test]
        public async Task Run_AppsettingsSecretsAreWrong_RepairIsOFF_ReturnsFailure()
        {
            _bironextLocationCheckMock.Setup(x => x.Run())
                .ReturnsAsync(new ResultRecord { Result = true });

            _sqlUtilsMock.Setup(x => x.ParseSqlServerToRealName(It.IsAny<string>()))
                .Returns(_sqlServerName);

            _directorySystemMock.SetupSequence(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(_secretsLocations.ToArray())
                .Returns(_appSettingsLocations.ToArray());

            _jsonParserMock.Setup(x => x.BuildConfig(It.IsAny<string>()))
                .Returns((string input) =>
                {
                    if (input.EndsWith(BiroNextConstants.NextSettingsSecretsFileName)) //appsettings.Secrets
                    {
                        if (input.Contains("identity_server", StringComparison.OrdinalIgnoreCase))
                            return _correctDefaultSecrets;
                        if (input.Contains("proxy_global", StringComparison.OrdinalIgnoreCase))
                            return _correctDefaultSecrets;
                        return _correctDefaultSecrets;
                    }
                    else //appsettings
                    {
                        if (input.Contains(Path.Combine(_nextRootFolder, BiroNextConstants.NextSettingsFileName), StringComparison.OrdinalIgnoreCase))
                            return _correctAppsettings;
                        return _correctDefaultAppsettings;
                    }
                });

            var result = await BuildCheck(false).Run();
            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo.Any(x => x.Contains(TextConstants.POSTFIX_ERROR) || result.AdditionalInfo.Any(x => x.Contains(TextConstants.POSTFIX_WARNING))), Is.True);
        }

        [Test]
        public async Task Run_AppsettingsAreWrong_RepairIsOFF_ReturnsFailure()
        {
            _bironextLocationCheckMock.Setup(x => x.Run())
                .ReturnsAsync(new ResultRecord { Result = true });

            _sqlUtilsMock.Setup(x => x.ParseSqlServerToRealName(It.IsAny<string>()))
                .Returns(_sqlServerName);

            _directorySystemMock.SetupSequence(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(_secretsLocations.ToArray())
                .Returns(_appSettingsLocations.ToArray());

            _jsonParserMock.Setup(x => x.BuildConfig(It.IsAny<string>()))
                .Returns((string input) =>
                {
                    if (input.EndsWith(BiroNextConstants.NextSettingsSecretsFileName)) //appsettings.Secrets
                    {
                        if (input.Contains("identity_server", StringComparison.OrdinalIgnoreCase))
                            return _correctSecrets;
                        if (input.Contains("proxy_global", StringComparison.OrdinalIgnoreCase))
                            return _correctSecrets;
                        return _correctDefaultSecrets;
                    }
                    else //appsettings
                    {
                        if (input.Contains(Path.Combine(_nextRootFolder, BiroNextConstants.NextSettingsFileName), StringComparison.OrdinalIgnoreCase))
                            return _correctDefaultAppsettings;
                        return _correctDefaultAppsettings;
                    }
                });

            var result = await BuildCheck(false).Run();
            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo.Any(x => x.Contains(TextConstants.POSTFIX_ERROR) || result.AdditionalInfo.Any(x => x.Contains(TextConstants.POSTFIX_WARNING))), Is.True);
        }

        [Test]
        public async Task Run_AppsettingsSecretsAreWrong_RepairIsON_ReturnsSuccess()
        {
            _bironextLocationCheckMock.Setup(x => x.Run())
                .ReturnsAsync(new ResultRecord { Result = true });

            _sqlUtilsMock.Setup(x => x.ParseSqlServerToRealName(It.IsAny<string>())) 
                .Returns(_sqlServerName);

            _directorySystemMock.SetupSequence(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(_secretsLocations.ToArray())
                .Returns(_appSettingsLocations.ToArray());

            _jsonParserMock.Setup(x => x.BuildConfig(It.IsAny<string>()))
                .Returns((string input) =>
                {
                    if (input.EndsWith(BiroNextConstants.NextSettingsSecretsFileName)) //appsettings.Secrets
                    {
                        if (input.Contains("identity_server", StringComparison.OrdinalIgnoreCase))
                            return _correctDefaultSecrets;
                        if (input.Contains("proxy_global", StringComparison.OrdinalIgnoreCase))
                            return _correctDefaultSecrets;
                        return _correctDefaultSecrets;
                    }
                    else //appsettings
                    {
                        if (input.Contains(Path.Combine(_nextRootFolder, BiroNextConstants.NextSettingsFileName), StringComparison.OrdinalIgnoreCase))
                            return _correctAppsettings;
                        return _correctDefaultAppsettings;
                    }
                });

            var result = await BuildCheck(true).Run();
            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo.Any(x => x.Contains(TextConstants.POSTFIX_REPAIR, StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public async Task Run_AppsettingsAreWrong_RepairIsON_ReturnsSuccess()
        {
            _bironextLocationCheckMock.Setup(x => x.Run())
                .ReturnsAsync(new ResultRecord { Result = true });

            _sqlUtilsMock.Setup(x => x.ParseSqlServerToRealName(It.IsAny<string>()))
                .Returns(_sqlServerName);

            _directorySystemMock.SetupSequence(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(_secretsLocations.ToArray())
                .Returns(_appSettingsLocations.ToArray());

            _jsonParserMock.Setup(x => x.BuildConfig(It.IsAny<string>()))
                .Returns((string input) =>
                {
                    if (input.EndsWith(BiroNextConstants.NextSettingsSecretsFileName)) //appsettings.Secrets
                    {
                        if (input.Contains("identity_server", StringComparison.OrdinalIgnoreCase))
                            return _correctSecrets;
                        if (input.Contains("proxy_global", StringComparison.OrdinalIgnoreCase))
                            return _correctSecrets;
                        return _correctDefaultSecrets;
                    }
                    else //appsettings
                    {
                        if (input.Contains(Path.Combine(_nextRootFolder, BiroNextConstants.NextSettingsFileName), StringComparison.OrdinalIgnoreCase))
                            return _correctDefaultAppsettings;
                        return _correctDefaultAppsettings;
                    }
                });

            var result = await BuildCheck(true).Run();
            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo.Any(x => x.Contains(TextConstants.POSTFIX_REPAIR, StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public async Task Run_MissingSettings_ReturnFailure()
        {
            _bironextLocationCheckMock.Setup(x => x.Run())
                .ReturnsAsync(new ResultRecord { Result = true });

            _sqlUtilsMock.Setup(x => x.ParseSqlServerToRealName(It.IsAny<string>()))
                .Returns(_sqlServerName);

            _directorySystemMock.SetupSequence(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(_secretsLocations.Take(2).ToArray())
                .Returns(_appSettingsLocations.ToArray());

            _jsonParserMock.Setup(x => x.BuildConfig(It.IsAny<string>()))
                .Returns((string input) =>
                {
                    if (input.EndsWith(BiroNextConstants.NextSettingsSecretsFileName)) //appsettings.Secrets
                    {
                        if (input.Contains("identity_server", StringComparison.OrdinalIgnoreCase))
                            return _correctSecrets;
                        if (input.Contains("proxy_global", StringComparison.OrdinalIgnoreCase))
                            return _correctSecrets;
                        return _correctDefaultSecrets;
                    }
                    else //appsettings
                    {
                        if (input.Contains(Path.Combine(_nextRootFolder, BiroNextConstants.NextSettingsFileName), StringComparison.OrdinalIgnoreCase))
                            return _correctDefaultAppsettings;
                        return _correctDefaultAppsettings;
                    }
                });

            var result = await BuildCheck(true).Run();
            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }
    }
}
