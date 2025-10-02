using common_ops.diagnostics.Checks.Environment.Checks;
using common_ops.diagnostics.Checks.Environment.Utils;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops_tests.EnvironmentChecksTests
{
    [TestFixture]
    public class Birokrat_Registry_CheckAndRepair_Test
    {
        Mock<IRegistrySettingsHelper> _registrySettingsHelperMock;
        Mock<IRegistryUtils> _registryUtilsMock;
        string _sqlServer;
        string _location;
        Dictionary<string, string> _correctRegistryDict;
        Dictionary<string, string> _incorrectRegistryDict;

        [SetUp]
        public void SetUp()
        {
            _registrySettingsHelperMock = new Mock<IRegistrySettingsHelper>();
            _registryUtilsMock = new Mock<IRegistryUtils>();

            _location = "location";
            _sqlServer = "SQLEXPRESS";

            _correctRegistryDict = new Dictionary<string, string>
            {
                { "Pot", _location },
                { "SQLServer", _sqlServer }
            };

            _incorrectRegistryDict = new Dictionary<string, string>
            {
                { "Pot", "" },
                { "SQLServer", "wrong" }
            };
        }

        private Birokrat_Registry_CheckAndRepair BuildCheck(bool doRepair)
        {
            return new Birokrat_Registry_CheckAndRepair(
                _registrySettingsHelperMock.Object,
                _registryUtilsMock.Object,
                _sqlServer,
                _location,
                doRepair,
                (message) => doRepair);
        }

        [Test]
        public async Task Run_RegistrySettingsAreCorrect_ReturnsSuccess()
        {
            _registrySettingsHelperMock.Setup(x => x.BuildCompareDictionary(_location, _sqlServer))
                .Returns(_correctRegistryDict);

            _registryUtilsMock.SetupSequence(x => x.GetRegistryValue(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_correctRegistryDict["Pot"])
                .Returns(_correctRegistryDict["SQLServer"]);

            _registryUtilsMock.Setup(x => x.FixRegistryValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { });

            var result = await BuildCheck(false).Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_RegistrySettingsAreIncorrectButRepairIsOn_ReturnsSuccess()
        {
            _registrySettingsHelperMock.Setup(x => x.BuildCompareDictionary(_location, _sqlServer))
                .Returns(_correctRegistryDict);

            _registryUtilsMock.SetupSequence(x => x.GetRegistryValue(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_incorrectRegistryDict["Pot"])
                .Returns(_correctRegistryDict["SQLServer"])
                .Returns("NULL");

            _registryUtilsMock.Setup(x => x.FixRegistryValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true)
                .Callback(() => { });

            _registrySettingsHelperMock.Setup(x => x.REGISTRY_KEY).Returns("reg\\key");

            var result = await BuildCheck(true).Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_RegistrySettingsAreIncorrectButRepairIsOff_ReturnsFailure()
        {
            _registrySettingsHelperMock.Setup(x => x.BuildCompareDictionary(_location, _sqlServer))
                .Returns(_correctRegistryDict);

            _registryUtilsMock.SetupSequence(x => x.GetRegistryValue(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_incorrectRegistryDict["Pot"])
                .Returns(_correctRegistryDict["SQLServer"]);

            _registryUtilsMock.Setup(x => x.FixRegistryValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false)
                .Callback(() => { });

            _registrySettingsHelperMock.Setup(x => x.REGISTRY_KEY).Returns("reg\\key");

            var result = await BuildCheck(false).Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_RegistrySettingsAreMissingButRepairIsOff_ReturnsFailure()
        {
            _registrySettingsHelperMock.Setup(x => x.BuildCompareDictionary(_location, _sqlServer))
                .Returns(new Dictionary<string, string>() { });

            _registryUtilsMock.SetupSequence(x => x.GetRegistryValue(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("Pot")
                .Returns(_correctRegistryDict["SQLServer"]);

            _registryUtilsMock.Setup(x => x.FixRegistryValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false)
                .Callback(() => { });

            _registrySettingsHelperMock.Setup(x => x.REGISTRY_KEY).Returns("reg\\key");

            var result = await BuildCheck(false).Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }
    }
}
