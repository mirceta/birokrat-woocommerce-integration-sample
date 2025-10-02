using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Next.Checks;
using common_ops.diagnostics.Checks.Next.Utils;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.NextChecksTests
{
    [TestFixture]
    public class BiroNext_BetaExe_Check_Tests
    {
        BiroNext_BetaExe_Check _check;
        Mock<ISimpleSettingsLoader> _settingsLoaderMock;
        Mock<IPathSystem> _pathSystemMock;
        Mock<IDirectorySystem> _directorySystemMock;

        Dictionary<string, string> _correctBiroInstancePoolSettings;
        Dictionary<string, string> _incorrectBiroInstancePoolSettings;
        List<string> _oneExeList;
        List<string> _twoExeList;
        List<string> _twoExeListWrong;

        [SetUp]
        public void SetUp()
        {
            _settingsLoaderMock = new Mock<ISimpleSettingsLoader>();
            _pathSystemMock = new Mock<IPathSystem>();
            _directorySystemMock = new Mock<IDirectorySystem>();

            _correctBiroInstancePoolSettings = new Dictionary<string, string> { { "birokrat_beta_name", "Birokrat.exe" } };
            _incorrectBiroInstancePoolSettings = new Dictionary<string, string> { { "birokrat_beta_name", "Birokrat5.exe" } };

            _oneExeList = new List<string> { "Birokrat.exe" };
            _twoExeList = new List<string> { "Birokrat.exe", "Birokrat2.exe" };
            _twoExeListWrong = new List<string> { "Birokrat3.exe", "Birokrat2.exe" };

            _check = new BiroNext_BetaExe_Check(_directorySystemMock.Object, _pathSystemMock.Object, _settingsLoaderMock.Object, "BaseDirectory");
        }

        [Test]
        public async Task Run_OnlyOneExeInBirokratFolder_CorrectSettings_ReturnsSuccess()
        {

            _settingsLoaderMock.Setup(x => x.LoadSettings(It.IsAny<string>()))
                .Returns(_correctBiroInstancePoolSettings);

            _directorySystemMock.Setup(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(_oneExeList);

            _pathSystemMock.Setup(x => x.GetFileName(It.IsAny<string>()))
                .Returns(_oneExeList.First());

            var result = await _check.Run();
            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_MultipleExeInBirokratFolder_CorrectSettings_ReturnsSuccess()
        {

            _settingsLoaderMock.Setup(x => x.LoadSettings(It.IsAny<string>()))
                .Returns(_correctBiroInstancePoolSettings);

            _directorySystemMock.Setup(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(_twoExeList);

            _pathSystemMock.SetupSequence(x => x.GetFileName(It.IsAny<string>()))
                .Returns(_twoExeList[0])
                .Returns(_twoExeList[1])
                .Returns(_twoExeList[0])
                .Returns(_twoExeList[1]);

            var result = await _check.Run();
            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_MultipleExeInBirokratFolder_WrongSettingsButCorrectBirokratExe_ReturnsSuccess()
        {

            _settingsLoaderMock.Setup(x => x.LoadSettings(It.IsAny<string>()))
                .Returns(_incorrectBiroInstancePoolSettings);

            _directorySystemMock.Setup(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(_twoExeList);

            _pathSystemMock.SetupSequence(x => x.GetFileName(It.IsAny<string>()))
                .Returns(_twoExeList[0])
                .Returns(_twoExeList[1])
                .Returns(_twoExeList[0])
                .Returns(_twoExeList[1]);

            var result = await _check.Run();
            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_MultipleExeInBirokratFolder_WrongSettingsAndWrongBirokratExe_ReturnsFailure()
        {
            _settingsLoaderMock.Setup(x => x.LoadSettings(It.IsAny<string>()))
                .Returns(_incorrectBiroInstancePoolSettings);

            _directorySystemMock.Setup(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(_twoExeListWrong);

            _pathSystemMock.SetupSequence(x => x.GetFileName(It.IsAny<string>()))
                .Returns(_twoExeListWrong[0])
                .Returns(_twoExeListWrong[1])
                .Returns(_twoExeListWrong[0])
                .Returns(_twoExeListWrong[1]);

            var result = await _check.Run();
            Assert.That(result.Result, Is.False);
        }

    }
}
