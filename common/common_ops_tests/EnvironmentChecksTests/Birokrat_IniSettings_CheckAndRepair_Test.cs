using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Environment.Checks;
using common_ops.diagnostics.Checks.Environment.Utils;
using common_ops.diagnostics.Checks.Location.Utils;
using common_ops.diagnostics.Constants;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.EnvironmentChecksTests
{
    [TestFixture]
    public class Birokrat_IniSettings_CheckAndRepair_Test
    {
        Mock<IIniSettingsHelper> _iniSettingsHelperMock;
        Mock<IFileSystem> _fileSystem;
        Mock<IReadonlySetter> _readOnlySetterMock;
        string _sqlServer;
        Dictionary<string, string> _correctIni;
        Dictionary<string, string> _wrongIni;


        [SetUp]
        public void SetUp()
        {
            _iniSettingsHelperMock = new Mock<IIniSettingsHelper>();
            _fileSystem = new Mock<IFileSystem>();
            _readOnlySetterMock = new Mock<IReadonlySetter>();

            _iniSettingsHelperMock.Setup(x => x.SaveIni(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Callback(() => { });
            _readOnlySetterMock.Setup(x => x.RemoveReadonlyFromFile(It.IsAny<string>()))
                .Callback(() => { });
            _readOnlySetterMock.Setup(x => x.MakeFileReadonly(It.IsAny<string>()))
                .Callback(() => { });

            _sqlServer = "SQLEXPRESS";

            _correctIni = new Dictionary<string, string>
            {
                { "SQN", $"{_sqlServer}" },
                { "NEXT", "-1" },
                { "Msgbox", "0" },
                { "RTC", "-1" },
                { "RTCOFF", "0" }
            };

            _wrongIni = new Dictionary<string, string>
            {
                { "SQN", $"" },
                { "NEXT", "-1" },
                { "RTC", "-1" },
                { "RTCOFF", "0" }
            };
        }

        private Birokrat_IniSettings_CheckAndRepair BuildCheck(bool doRepair)
        {
            return new Birokrat_IniSettings_CheckAndRepair(
                _iniSettingsHelperMock.Object,
                _readOnlySetterMock.Object,
                _fileSystem.Object,
                _sqlServer,
                "location",
                doRepair);
        }

        [Test]
        public async Task Run_AllSettingsAreOk_ReturnsSuccess()
        {
            _fileSystem.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
            _fileSystem.Setup(x => x.WriteAllLines(It.IsAny<string>(), It.IsAny<List<string>>())).Callback(() => { });
            _fileSystem.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns("neki neki");

            _iniSettingsHelperMock.Setup(x => x.BuildCompareDictionary(It.IsAny<string>()))
                .Returns(_correctIni);

            _iniSettingsHelperMock.Setup(x => x.GenerateDictionaryFromIni_FixDuplicateValues(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(_correctIni);

            _iniSettingsHelperMock.Setup(x => x.BuildNotWantedKeysArray())
               .Returns(new string[] { });

            var result = await BuildCheck(false).Run();

            Assert.That(result.Result, Is.True);

        }

        [Test]
        public async Task Run_SettingsWrongButRepairIsOn_ReturnsSuccess()
        {
            _fileSystem.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
            _fileSystem.Setup(x => x.WriteAllLines(It.IsAny<string>(), It.IsAny<List<string>>())).Callback(() => { });
            _fileSystem.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns("neki neki");

            _iniSettingsHelperMock.Setup(x => x.BuildCompareDictionary(It.IsAny<string>()))
                .Returns(_correctIni);

            _iniSettingsHelperMock.Setup(x => x.GenerateDictionaryFromIni_FixDuplicateValues(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(_wrongIni);

            _iniSettingsHelperMock.Setup(x => x.BuildNotWantedKeysArray())
               .Returns(new string[] { });

            var result = await BuildCheck(true).Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_SettingsWrongButRepairIsOff_ReturnsFailure()
        {
            _fileSystem.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
            _fileSystem.Setup(x => x.WriteAllLines(It.IsAny<string>(), It.IsAny<List<string>>())).Callback(() => { });
            _fileSystem.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns("neki neki");

            _iniSettingsHelperMock.Setup(x => x.BuildCompareDictionary(It.IsAny<string>()))
                .Returns(_correctIni);

            _iniSettingsHelperMock.Setup(x => x.GenerateDictionaryFromIni_FixDuplicateValues(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(_wrongIni);

            _iniSettingsHelperMock.Setup(x => x.BuildNotWantedKeysArray())
               .Returns(new string[] { });

            var result = await BuildCheck(false).Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_IniSettingsNotFound_RegeneratesIni_ReturnsSuccess()
        {
            _fileSystem.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
            _fileSystem.Setup(x => x.WriteAllLines(It.IsAny<string>(), It.IsAny<List<string>>())).Callback(() => { });
            _fileSystem.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns("neki neki");

            _iniSettingsHelperMock.Setup(x => x.BuildCompareDictionary(It.IsAny<string>()))
                .Returns(new Dictionary<string, string>() { });

            _iniSettingsHelperMock.Setup(x => x.GenerateDictionaryFromIni_FixDuplicateValues(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(_correctIni);

            _iniSettingsHelperMock.Setup(x => x.BuildNotWantedKeysArray())
               .Returns(new string[] { });

            var result = await BuildCheck(false).Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo.FirstOrDefault().Contains(TextConstants.POSTFIX_REPAIR), Is.True);
        }
    }
}
