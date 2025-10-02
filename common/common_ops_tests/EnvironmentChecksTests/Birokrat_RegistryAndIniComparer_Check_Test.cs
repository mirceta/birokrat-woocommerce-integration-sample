using common_ops.diagnostics;
using common_ops.diagnostics.Checks.Environment.Checks;
using common_ops.diagnostics.Checks.Environment.Utils;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops_tests.EnvironmentChecksTests
{
    [TestFixture]
    public class Birokrat_RegistryAndIniComparer_Check_Test
    {
        Mock<IRegistrySettingsHelper> _registrySettingsHelperMock;
        Mock<IIniSettingsHelper> _iniSettingsHelperMock;
        Mock<IRegistryUtils> _registryUtilsMock;
        string _sqlServer;
        string _location;
        Dictionary<string, string> _correctRegistryDict;
        Dictionary<string, string> _incorrectRegistryDict;
        Dictionary<string, string> _correctIniDict;
        Dictionary<string, string> _wrongIniDict;


        [SetUp]
        public void SetUp()
        {
            _registrySettingsHelperMock = new Mock<IRegistrySettingsHelper>();
            _registryUtilsMock = new Mock<IRegistryUtils>();
            _iniSettingsHelperMock = new Mock<IIniSettingsHelper>();

            _location = "location";
            _sqlServer = "SQLEXPRESS";

            _correctRegistryDict = new Dictionary<string, string>
            {
                { "Pot", _location },
                { "SQLServer", _sqlServer }
            };

            _incorrectRegistryDict = new Dictionary<string, string>
            {
                { "Pot", "Wrong" },
                { "SQLServer", "wrong" }
            };

            _correctIniDict = new Dictionary<string, string>
            {
                { "SQN", _sqlServer },
                { "NEXT", "-1" },
                { "Msgbox", "0" },
                { "RTC", "-1" },
                { "RTCOFF", "0" }
            };

            _wrongIniDict = new Dictionary<string, string>
            {
                { "SQN", $"" },
                { "NEXT", "-1" },
                { "RTC", "0" },
                { "RTCOFF", "0" }
            };
        }

        private Birokrat_RegistryAndIniComparer_Check BuildCheck()
        {
            return new Birokrat_RegistryAndIniComparer_Check(
               _iniSettingsHelperMock.Object,
               _registrySettingsHelperMock.Object,
               _registryUtilsMock.Object,
               _sqlServer,
               _location);
        }

        [Test]
        public async Task Run_IniAndRegistryValuesMatch_ReturnsSuccess()
        {
            _iniSettingsHelperMock.Setup(x => x.BuildCompareDictionary(_sqlServer))
                .Returns(_correctIniDict);

            _iniSettingsHelperMock.Setup(x => x.GenerateDictionaryFromIni_FixDuplicateValues(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(_correctIniDict);

            _registryUtilsMock.Setup(x => x.GetRegistryValue(It.IsAny<string>(), It.Is<string>(key => key == "SQN")))
                .Returns(_correctRegistryDict["SQLServer"]);

            var result = await BuildCheck().Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_WrongRegistrySettings_ReturnsFailure()
        {
            _iniSettingsHelperMock.Setup(x => x.BuildCompareDictionary(_sqlServer))
                .Returns(_correctIniDict);

            _iniSettingsHelperMock.Setup(x => x.GenerateDictionaryFromIni_FixDuplicateValues(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(_correctIniDict);

            _registryUtilsMock.Setup(x => x.GetRegistryValue(It.IsAny<string>(), It.Is<string>(key => key == "SQN")))
                .Returns(_incorrectRegistryDict["SQLServer"]);

            var result = await BuildCheck().Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_WrongInitSettings_ReturnsFailure()
        {
            _iniSettingsHelperMock.Setup(x => x.BuildCompareDictionary(It.IsAny<string>()))
                .Returns(_correctIniDict);

            _iniSettingsHelperMock.Setup(x => x.GenerateDictionaryFromIni_FixDuplicateValues(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(_wrongIniDict);

            _registryUtilsMock.Setup(x => x.GetRegistryValue(It.IsAny<string>(), It.Is<string>(key => key == "SQN")))
                .Returns(_correctRegistryDict["SQLServer"]);

            var result = await BuildCheck().Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_NoIniSettings_ReturnsFailure()
        {
            _iniSettingsHelperMock.Setup(x => x.BuildCompareDictionary(It.IsAny<string>()))
                .Returns(_correctIniDict);

            _iniSettingsHelperMock.Setup(x => x.GenerateDictionaryFromIni_FixDuplicateValues(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(new Dictionary<string, string>());

            _registryUtilsMock.Setup(x => x.GetRegistryValue(It.IsAny<string>(), It.Is<string>(key => key == "SQN")))
                .Returns(_correctRegistryDict["SQLServer"]);

            var result = await BuildCheck().Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }
    }
}
