using common_ops.diagnostics.Checks.Next.Checks;
using common_ops.FileHandler;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace common_ops_tests.NextChecksTests
{
    [TestFixture]
    public class BiroNext_FileCountVerifier_Check_Tests
    {
        BiroNext_FileCountVerifier_Check _check;
        Mock<IDirectoryContentHandler> _directoryContentHandlerMock;
        string _sourceDirectory;
        string _originDirectory;

        [SetUp]
        public void SetUp()
        {
            _originDirectory = "origin";
            _sourceDirectory = "source";

           _directoryContentHandlerMock = new Mock<IDirectoryContentHandler>();

            _check = new BiroNext_FileCountVerifier_Check(_directoryContentHandlerMock.Object, _sourceDirectory, _originDirectory);
        }

        [Test]
        public async Task Run_SameFileCount_ReturnsSuccess()
        {
            _directoryContentHandlerMock.Setup(x => x.GetTotalFilesInDirectory(It.IsAny<string>()))
                    .Returns(10);

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_SourceFileCountIsLarger_ReturnsSuccess()
        {

            int callCount = 0;
            _directoryContentHandlerMock.Setup(x => x.GetTotalFilesInDirectory(It.IsAny<string>()))
                    .Returns(() =>
                    {
                        callCount++;
                        return callCount == 1 ? 10 : 5;
                    });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_OriginFileCountIsLarger_ReturnsFailure()
        {

            int callCount = 0;
            _directoryContentHandlerMock.Setup(x => x.GetTotalFilesInDirectory(It.IsAny<string>()))
                     .Returns(() =>
                     {
                         callCount++;
                         return callCount == 1 ? 5 : 10;
                     });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }
    }
}
