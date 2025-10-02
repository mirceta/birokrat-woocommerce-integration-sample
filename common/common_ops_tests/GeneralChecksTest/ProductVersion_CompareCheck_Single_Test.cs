using common_ops.Abstractions;
using common_ops.diagnostics.Checks.General.Checks;
using common_ops.diagnostics.Checks.General.Utils;
using common_ops.diagnostics.Constants;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.GeneralChecksTest
{
    [TestFixture]
    public class ProductVersion_CompareCheck_Single_Test
    {
        private readonly string SOURCE = "source.txt";
        private readonly string LOCAL = "local.txt";

        ProductVersion_CompareCheck_Single _check;
        Mock<IFileVersionExtractor> _versionExtractorMock;
        Mock<IFileSystem> _fileSystemMock;

        MockTextFileCreator local;
        MockTextFileCreator source;

        [OneTimeSetUp]
        public void CreateMockFiles()
        {
            local = new MockTextFileCreator(LOCAL);
            source = new MockTextFileCreator(SOURCE, true);
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            local.Dispose();
            source.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _versionExtractorMock = new Mock<IFileVersionExtractor>();
            _fileSystemMock = new Mock<IFileSystem>();

            _check = new ProductVersion_CompareCheck_Single(
                _versionExtractorMock.Object,
                _fileSystemMock.Object,
                LOCAL,
                SOURCE);
        }

        [Test]
        public async Task Run_SameVersionAndCreationDateMatch_ReturnsSuccess()
        {
            var expectedVersion = new Version(1, 0, 0, 0);

            _versionExtractorMock.Setup(x => x.TryGetVersion(out expectedVersion, It.IsAny<string>()))
                .Returns(true);

            _fileSystemMock.Setup(x => x.GetFileInfo(It.IsAny<string>()))
                .Returns(source.FileInfo);

            _fileSystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_FileNotFound_ReturnsFailure()
        {
            var expectedVersion = new Version(1, 0, 0, 0);

            _versionExtractorMock.Setup(x => x.TryGetVersion(out expectedVersion, It.IsAny<string>()))
                .Returns(true);

            _fileSystemMock.Setup(x => x.GetFileInfo(It.IsAny<string>()))
                .Returns(source.FileInfo);

            _fileSystemMock.SetupSequence(x => x.Exists(It.IsAny<string>()))
                .Returns(true)
                .Returns(false);

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_VersionsDontMatch_ReturnsFailure()
        {
            var expectedVersion = new Version(2, 0, 0);
            var oldVersion = new Version(1, 0, 0);

            _versionExtractorMock.Setup(x => x.TryGetVersion(out expectedVersion, It.IsAny<string>()))
                .Returns(true);

            _versionExtractorMock.Setup(x => x.TryGetVersion(out oldVersion, It.IsAny<string>()))
               .Returns(false);

            _fileSystemMock.Setup(x => x.GetFileInfo(It.IsAny<string>()))
                .Returns(source.FileInfo);

            _fileSystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_VersionsMatchDifferentDates_ReturnsWarning()
        {
            var expectedVersion = new Version(1, 0, 0, 0);

            _versionExtractorMock.Setup(x => x.TryGetVersion(out expectedVersion, It.IsAny<string>()))
                .Returns(true);

            _fileSystemMock.SetupSequence(x => x.GetFileInfo(It.IsAny<string>()))
                .Returns(source.FileInfo)
                .Returns(local.FileInfo);

            _fileSystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                 .Returns(true);

            var result = await _check.Run();

            Assert.That(result.AdditionalInfo.Any(x => x.EndsWith(TextConstants.POSTFIX_WARNING)), Is.True);
        }

        [Test]
        public async Task Run_CouldNotExtractVersions_ReturnsFailure()
        {
            var expectedVersion = new Version(1, 0, 0, 0);

            _versionExtractorMock.Setup(x => x.TryGetVersion(out expectedVersion, It.IsAny<string>()))
                .Returns(false);

            _fileSystemMock.Setup(x => x.GetFileInfo(It.IsAny<string>()))
                .Returns(source.FileInfo);

            _fileSystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }
    }
}
