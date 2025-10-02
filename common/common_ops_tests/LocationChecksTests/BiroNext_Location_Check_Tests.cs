using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Location.Checks;
using common_ops.diagnostics.Checks.Location.Utils;
using Moq;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.LocationChecksTests
{
    [TestFixture]
    public class BiroNext_Location_Check_Tests
    {
        private BiroNext_Location_Check _check;
        private Mock<IFileSystem> _fileSystem;
        private Mock<ILocationHelper> _locationHelper;
        private Mock<IDirectorySystem> _directorySystem;
        private string _location;

        [SetUp]
        public void SetUp()
        {
            _location = "C:\\TestLocation";
            _fileSystem = new Mock<IFileSystem>();
            _directorySystem = new Mock<IDirectorySystem>();
            _locationHelper = new Mock<ILocationHelper>();

            _check = new BiroNext_Location_Check(_locationHelper.Object, _fileSystem.Object, _directorySystem.Object, _location);
        }

        [Test]
        public async Task Run_IsNextInstalationValid_ReturnsSuccessResult()
        {
            var dirInLocation = new DirectoryInfo[]
            {
                new DirectoryInfo("C\\Folder1"),
                new DirectoryInfo("C\\Folder2"),
            };

            _fileSystem.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            _directorySystem.Setup(x => x.GetDirectoriesInfo(It.IsAny<string>()))
                .Returns(dirInLocation);

            _locationHelper.Setup(x => x.AreAllRequiredFoldersPresent(It.IsAny<DirectoryInfo[]>(), It.IsAny<string[]>()))
                .Returns((true, new string[] { }));

            _locationHelper.Setup(x => x.CheckIfFolderExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((_location, true));

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(_location.Equals(result.AdditionalInfo.FirstOrDefault(), System.StringComparison.OrdinalIgnoreCase), Is.True);
        }

        [Test]
        public async Task Run_IsNextNotPresent_ReturnFailedResult()
        {
            var dirInLocation = new DirectoryInfo[]
            {
                new DirectoryInfo("C\\Folder1"),
                new DirectoryInfo("C\\Folder2"),
            };

            _fileSystem.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(false);

            _directorySystem.Setup(x => x.GetDirectoriesInfo(It.IsAny<string>()))
                .Returns(dirInLocation);

            _locationHelper.Setup(x => x.AreAllRequiredFoldersPresent(It.IsAny<DirectoryInfo[]>(), It.IsAny<string[]>()))
                .Returns((true, new string[] { }));

            _locationHelper.Setup(x => x.CheckIfFolderExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((_location, true));

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(string.IsNullOrEmpty(result.AdditionalInfo.FirstOrDefault()), Is.True);
        }

        [Test]
        public async Task Run_IsNextPresentButNotAllServices_ReturnFailedResult()
        {
            var dirInLocation = new DirectoryInfo[]
            {
                new DirectoryInfo("C\\Folder1"),
                new DirectoryInfo("C\\Folder2"),
            };

            _fileSystem.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            _directorySystem.Setup(x => x.GetDirectoriesInfo(It.IsAny<string>()))
                .Returns(dirInLocation);

            _locationHelper.Setup(x => x.AreAllRequiredFoldersPresent(It.IsAny<DirectoryInfo[]>(), It.IsAny<string[]>()))
                .Returns((false, new string[] { "folder missing" }));

            _locationHelper.Setup(x => x.CheckIfFolderExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((_location, true));

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_IsNextFolderPresent_ReturnFailedResult()
        {
            var dirInLocation = new DirectoryInfo[]
            {
                new DirectoryInfo("C\\Folder1"),
                new DirectoryInfo("C\\Folder2"),
            };

            _fileSystem.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(false);

            _directorySystem.Setup(x => x.GetDirectoriesInfo(It.IsAny<string>()))
                .Returns(dirInLocation);

            _locationHelper.Setup(x => x.AreAllRequiredFoldersPresent(It.IsAny<DirectoryInfo[]>(), It.IsAny<string[]>()))
                .Returns((true, new string[] { "folder missing" }));

            _locationHelper.Setup(x => x.CheckIfFolderExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((_location, false));

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }
    }
}
