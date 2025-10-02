using common_ops.Abstractions;
using common_ops.diagnostics.Checks.General.Checks;
using common_ops.diagnostics.Checks.General.Utils;
using common_ops.diagnostics.Constants;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.GeneralChecksTest
{
    [TestFixture]
    public class ProductVersion_CompareCheck_Multiple_Tests
    {
        private readonly string SOURCE = "source.txt";
        private readonly string LOCAL = "local.txt";

        ProductVersion_CompareCheck_Multiple _check;
        Mock<IFileVersionExtractor> _versionExtractorMock;
        Mock<IFileSystem> _fileSystemMock;
        Mock<IDirectorySystem> _directorySystemMock;
        Func<string, string, ProductVersion_CompareCheck_Single> _singleFactory;

        MockTextFileCreator local;
        MockTextFileCreator source;

        [OneTimeSetUp]
        public void CreateMockFiles()
        {
            local = new MockTextFileCreator(LOCAL);
            source = new MockTextFileCreator(SOURCE);
        }

        [SetUp]
        public void SetUp()
        {

            _directorySystemMock = new Mock<IDirectorySystem>();
            _fileSystemMock = new Mock<IFileSystem>();
            _versionExtractorMock = new Mock<IFileVersionExtractor>();

            _singleFactory = (local, source) => new ProductVersion_CompareCheck_Single(
                _versionExtractorMock.Object,
                _fileSystemMock.Object,
                local,
                source);

            _check = new ProductVersion_CompareCheck_Multiple(
                _singleFactory,
                _directorySystemMock.Object,
                _fileSystemMock.Object,
                LOCAL,
                "BasePath");
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            local.Dispose();
            source.Dispose();
        }

        [Test]
        public async Task Run_SameVersionAndCreationDateMatch_ReturnsSuccess()
        {
            var version1 = new Version(1, 0, 0, 0);
            var version2 = new Version(2, 0, 0, 0);

            _versionExtractorMock.Setup(x => x.TryGetVersion(out version1, It.IsAny<string>()))
                .Returns(true);

            _directorySystemMock.Setup(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(new string[] { LOCAL, LOCAL });

            _directorySystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            _fileSystemMock.Setup(x => x.GetFileInfo(It.IsAny<string>()))
                .Returns(source.FileInfo);

            _fileSystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_SourceFileNotFound_ReturnsFailure()
        {
            _directorySystemMock.Setup(x => x.Exists(It.IsAny<string>()))
               .Returns(true);

            _fileSystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(false);

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_RootDirectoryNotFound_ReturnsWarning()
        {
            _directorySystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(false);

            _fileSystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }

        delegate void TryGetVersionCallback(out Version version, string path);

        [Test]
        public async Task Run_NoFilesInRootDirectory_ReturnsWarning()
        {
            var version1 = new Version(1, 0, 0, 0);
            var version2 = new Version(2, 0, 0, 0);
            var verCallCount = 0;

            _versionExtractorMock.Setup(x => x.TryGetVersion(out It.Ref<Version>.IsAny, It.IsAny<string>()))
                .Callback(new TryGetVersionCallback((out Version ver, string path) =>
                {
                    if (verCallCount == 0) ver = version2;
                    if (verCallCount == 1) ver = version2;
                    else ver = version1;
                    verCallCount++;
                }))
                .Returns(true);

            _directorySystemMock.Setup(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(new string[] { });

            _directorySystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            _fileSystemMock.Setup(x => x.GetFileInfo(It.IsAny<string>()))
                .Returns(source.FileInfo);

            _fileSystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            var result = await _check.Run();

            var checkResult = result.AdditionalInfo.Any(x => x.Contains(TextConstants.POSTFIX_WARNING, StringComparison.OrdinalIgnoreCase));
            Assert.That(checkResult, Is.True);
        }
    }
}
