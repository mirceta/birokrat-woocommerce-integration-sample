using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Location.Checks;
using common_ops.diagnostics.Checks.Location.Utils;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.LocationChecksTests
{
    [TestFixture]
    public class BirokratDll_Location_Check_Tests
    {
        string _location;
        BirokratDll_Location_Check _check; 
        Mock<IDirectorySystem> _directorySystem;
        Mock<ILocationHelper> _locationHelper;

        [SetUp]
        public void SetUp()
        {
            _location = "C:\\DllLocation";
            _locationHelper = new Mock<ILocationHelper>();
            _directorySystem = new Mock<IDirectorySystem>();
            _check = new BirokratDll_Location_Check(_locationHelper.Object, _directorySystem.Object, _location);
        }

        [Test]
        public async Task Run_DllLocationIsValid_ReturnsSuccess()
        {
            var directories = new DirectoryInfo[]
            {
                new DirectoryInfo($"C:\\{_location}\\Dir1"),
                new DirectoryInfo($"C:\\{_location}\\Dir2")
            };

            _directorySystem.Setup(x => x.GetDirectoriesInfo(It.IsAny<string>()))
                .Returns(directories);

            _locationHelper.Setup(x => x.AreAllRequiredFoldersPresent(It.IsAny<DirectoryInfo[]>(), It.IsAny<string[]>()))
                .Returns((true, new string[] { }));

            _locationHelper.Setup(x => x.CheckIfFolderExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((_location, true));

            var result = await _check.Run();

            Assert.IsTrue(result.Result);
            Assert.AreEqual(_location, result.AdditionalInfo.FirstOrDefault());
        }

        [Test]
        public async Task Run_DllDirectoryDoesNotExist_ReturnsFailure()
        {
            _directorySystem.Setup(x => x.GetDirectoriesInfo(It.IsAny<string>()))
                .Returns(new DirectoryInfo[] { });

            _locationHelper.Setup(x => x.AreAllRequiredFoldersPresent(It.IsAny<DirectoryInfo[]>(), It.IsAny<string[]>()))
                .Returns((true, new string[] { }));

            _locationHelper.Setup(x => x.CheckIfFolderExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((_location, false));

            var result = await _check.Run();

            Assert.IsFalse(result.Result);
        }

        [Test]
        public async Task Run_DirectoryExistsButFoldersAreMissing_ReturnsFailure()
        {
            var directories = new DirectoryInfo[]
            {
                new DirectoryInfo($"C:\\{_location}\\Dir1"),
                new DirectoryInfo($"C:\\{_location}\\Dir2")
            };

            _directorySystem.Setup(x => x.GetDirectoriesInfo(It.IsAny<string>()))
                .Returns(new DirectoryInfo[] { });

            _locationHelper.Setup(x => x.AreAllRequiredFoldersPresent(It.IsAny<DirectoryInfo[]>(), It.IsAny<string[]>()))
                .Returns((false, new string[] { "file missing" }));

            _locationHelper.Setup(x => x.CheckIfFolderExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((_location, true));

            var result = await _check.Run();

            Assert.IsFalse(result.Result);
        }
    }
}
