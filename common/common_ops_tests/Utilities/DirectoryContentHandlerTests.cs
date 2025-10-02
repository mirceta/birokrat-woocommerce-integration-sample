using common_ops.Abstractions;
using common_ops.FileHandler;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace common_ops_tests.Utilities
{
    [TestFixture]
    public class DirectoryContentHandlerTests
    {
        DirectoryContentHandler _handler;
        Mock<IFileSystem> _fileSystemMock;
        Mock<IDirectorySystem> _directorySystemMock;
        Mock<ICopyFileWithProgress> _copyFilesWithProgressMock;
        Mock<IPathSystem> _pathSystemMock;
        List<string> _loggerMock;
        List<string> _operations;

        string[] _directories, _files;
        string _baseDir, _targetDir;

        [SetUp]
        public void SetUp()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _directorySystemMock = new Mock<IDirectorySystem>();
            _pathSystemMock = new Mock<IPathSystem>();

            _copyFilesWithProgressMock = new Mock<ICopyFileWithProgress>();
            _loggerMock = new List<string>();
            _operations = new List<string>();

            _baseDir = "BaseDirectory";
            _targetDir = "TargetDirectory";

            _handler = new DirectoryContentHandler(
                _fileSystemMock.Object,
                _directorySystemMock.Object,
                _pathSystemMock.Object,
                _copyFilesWithProgressMock.Object,
                (message) => _loggerMock.Add(message));
        }

        [Test]
        public async Task CopyDirectoryAsync_AllFilesCopied_ReturnsSuccess()
        {
            //this test will mock copy data into _operations. I will copy into 3 folders: _targetDir and 2 directories defined in _directories. All 3 folders must have 2 files defined in _files

            _directories = new string[] { "dir1", "dir2" };
            _files = new string[] { "AAA.file", "BBB.file" };

            _directorySystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            _fileSystemMock.Setup(x => x.GetFileSize(It.IsAny<string>()))
                .Returns(1024);

            _pathSystemMock.Setup(x => x.GetFileExtension(It.IsAny<string>()))
                .Returns(".file");

            _directorySystemMock.Setup(x => x.CreateDirectory(It.IsAny<string>()))
                .Callback<string>((path) => _operations.Add(path));

            _pathSystemMock.Setup(x => x.GetFileName(It.IsAny<string>()))
                .Returns<string>((name) => name);

            var callIndex = 0;
            _directorySystemMock.Setup(x => x.GetDirectories(It.IsAny<string>()))
                .Returns(() =>
                {
                    Interlocked.Increment(ref callIndex);
                    return callIndex % 3 == 1 ? _directories : Array.Empty<string>();
                });

            _directorySystemMock.Setup(x => x.GetFiles(It.IsAny<string>(), "*", System.IO.SearchOption.TopDirectoryOnly))
                .Returns(_files);

            _copyFilesWithProgressMock.Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((from, to) => _operations.Add(from + " -> " + to));

            await _handler.CopyDirectoryAsync(_baseDir, _targetDir, true);

            Assert.That(_operations.Count == 9, Is.True);
            Assert.That(_operations.Any(x => string.IsNullOrEmpty(x)), Is.False);
        }

        [Test]
        public async Task GetTotalFilesInDirectory_CountAllFilerRecoursive_ReturnsSuccess()
        {
            //this test will mock copy data into _operations. I will copy into 3 folders: _targetDir and 2 directories defined in _directories. All 3 folders must have 2 files defined in _files

            _directories = new string[] { "dir1", "dir2" };
            _files = new string[] { "AAA.file", "BBB.file" };

            _directorySystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            _fileSystemMock.Setup(x => x.GetFileSize(It.IsAny<string>()))
                .Returns(1024);

            _pathSystemMock.Setup(x => x.GetFileExtension(It.IsAny<string>()))
                .Returns(".file");

            var dirIndex1 = 0;
            _directorySystemMock.Setup(x => x.GetDirectories(It.IsAny<string>()))
                .Returns(() =>
                {
                    dirIndex1++;
                    return dirIndex1 == 1 ? _directories : Array.Empty<string>();
                });

            _directorySystemMock.Setup(x => x.GetFiles(It.IsAny<string>(), "*", System.IO.SearchOption.TopDirectoryOnly))
                .Returns(_files);

            _directorySystemMock.Setup(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(_files);

            var dirIndex2 = 0;
            _directorySystemMock.Setup(x => x.IsAnyDirectory(It.IsAny<string>()))
                .Returns(() =>
                {
                    dirIndex2++;
                    return dirIndex2 == 1;
                });

            var result = _handler.GetTotalFilesInDirectory(_baseDir);

            Assert.That(result == 6, Is.True);
        }

        [Test]
        public async Task DeleteAllContent_DeletesAllFilesAndDirectories_ReturnsSuccess()
        {
            //this test will mock copy data into _operations. I will copy into 3 folders: _targetDir and 2 directories defined in _directories. All 3 folders must have 2 files defined in _files

            _directories = new string[] { "dir1", "dir2" };
            _files = new string[] { "AAA.file", "BBB.file" };

            _directorySystemMock.Setup(x => x.Exists(It.IsAny<string>()))
                .Returns(true);

            _pathSystemMock.Setup(x => x.GetFileNameWithoutExtension(It.IsAny<string>()))
                .Returns((string file) => file.Split('.').FirstOrDefault());

            var dirIndex1 = 0;
            _directorySystemMock.Setup(x => x.GetDirectories(It.IsAny<string>()))
                .Returns(() =>
                {
                    dirIndex1++;
                    return dirIndex1 == 1 ? _directories : Array.Empty<string>();
                });

            _directorySystemMock.Setup(x => x.GetFiles(It.IsAny<string>(), "*", System.IO.SearchOption.TopDirectoryOnly))
                .Returns(_files);

            _directorySystemMock.Setup(x => x.Delete(It.IsAny<string>(), true))
                .Callback(() => { });

            var result = _handler.DeleteAllContent(_baseDir);

            Assert.That(result.Count == 4, Is.True);
            Assert.That(result.Any(x => string.IsNullOrEmpty(x)), Is.False);
        }
    }
}
