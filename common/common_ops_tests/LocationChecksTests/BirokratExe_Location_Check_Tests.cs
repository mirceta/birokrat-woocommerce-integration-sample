using common_ops.diagnostics.Checks.Location.Checks;
using common_ops.diagnostics.Checks.Location.Utils;
using NUnit.Framework;
using System.Threading.Tasks;
using System;
using Moq;
using System.Linq;
using common_ops.Abstractions;

namespace common_ops_tests.LocationChecksTests
{
    [TestFixture]
    public class BirokratExe_Location_Check_Tests
    {
        private Mock<ILocationHelper> _locationHelperMock;
        private Mock<IFileSystem> _fileSystemMock;
        private string _testLocation;
        private BirokratExe_Location_Check _check;

        [SetUp]
        public void Setup() 
        {
            _locationHelperMock = new Mock<ILocationHelper>();
            _fileSystemMock = new Mock<IFileSystem>();
            _testLocation = "C:\\TestLocation";

            _check = new BirokratExe_Location_Check(_locationHelperMock.Object, _fileSystemMock.Object, _testLocation);
        }

        [Test]
        public async Task Run_FolderAndFileExist_ReturnsSuccessResult()
        {
            // Arrange
            _locationHelperMock.Setup(m => m.CheckIfFolderExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((_testLocation, true));
            _fileSystemMock.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);

            // Act
            var result = await _check.Run();

            // Assert
            Assert.That(result.Result, Is.True);
            Assert.That(_testLocation.Equals(result.AdditionalInfo.FirstOrDefault(), StringComparison.OrdinalIgnoreCase), Is.True);
        }

        [Test]
        public async Task Run_FolderExistsButFileDoesNot_ReturnsFailureResult()
        {
            // Arrange
            _locationHelperMock.Setup(m => m.CheckIfFolderExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((_testLocation, true));
            _fileSystemMock.Setup(m => m.Exists(It.IsAny<string>())).Returns(false);

            // Act
            var result = await _check.Run();

            // Assert
            Assert.That(result.Result, Is.False);
            Assert.That(string.IsNullOrEmpty(result.AdditionalInfo.FirstOrDefault()), Is.True);
        }

        [Test]
        public async Task Run_NeitherFolderNorFileExists_ReturnsFailureResult()
        {
            // Arrange
            _locationHelperMock.Setup(m => m.CheckIfFolderExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((_testLocation, false));

            // Act
            var result = await _check.Run();

            // Assert
            Assert.That(result.Result, Is.False);
            Assert.That(string.IsNullOrEmpty(result.AdditionalInfo.FirstOrDefault()), Is.True);
        }

        [Test]
        public async Task Run_ThrowsException_ReturnsErrorResult()
        {
            // Arrange
            _locationHelperMock.Setup(m => m.CheckIfFolderExists(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _check.Run();

            // Assert
            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo.FirstOrDefault().Contains("Error: Test exception", StringComparison.OrdinalIgnoreCase), Is.True);
        }
    }
}
