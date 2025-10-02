using common_ops.diagnostics.Checks.Location.Utils;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace common_ops_tests.LocationChecksTests
{
    [TestFixture]
    public class LocationHelperTests
    {

        private LocationHelper _locationHelper;
        private readonly string[] REQUIRED_FILES = new string[] { "ble", "flj" };
        private readonly string[] REQUIRED_FOLDERS = new string[] { "Data", "Objects" };

        [SetUp]
        public void SetUp()
        {
            _locationHelper = new LocationHelper();
        }

        [Test]
        public void AreAllRequiredFoldersPresent_FoldersArePresent_ReturnsSuccess()
        {
            var local = new DirectoryInfo[]
            {
                new DirectoryInfo("C:\\Data"),
                new DirectoryInfo("C:\\Objects")
            };

            var result = _locationHelper.AreAllRequiredFoldersPresent(local, REQUIRED_FOLDERS);

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public void AreAllRequiredFoldersPresent_ManyLocalFiles_ReturnsSuccess()
        {
            var local = new DirectoryInfo[]
            {
                new DirectoryInfo("C:\\Data"),
                new DirectoryInfo("C:\\Objects"),
                new DirectoryInfo("C:\\lib"),
                new DirectoryInfo("C:\\bin"),
                new DirectoryInfo("C:\\obj")
            };

            var result = _locationHelper.AreAllRequiredFoldersPresent(local, REQUIRED_FOLDERS);

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public void AreAllRequiredFoldersPresent_OneFolderIsMissing_ReturnsFailure()
        {
            var local = new DirectoryInfo[]
            {
                new DirectoryInfo("C:\\Data")
            };

            var result = _locationHelper.AreAllRequiredFoldersPresent(local, REQUIRED_FOLDERS);

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public void AreAllRequiredFoldersPresent_NoLocalFolders_ReturnsFailure()
        {
            var local = new DirectoryInfo[]
            {

            };

            var result = _locationHelper.AreAllRequiredFoldersPresent(local, REQUIRED_FOLDERS);

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public void AreAllRequiredFilesPresent_FilesArePresent_ReturnsSuccess()
        {
            var local = new FileInfo[]
            {
                 new FileInfo("C:\\Data\\ble.exe"),
                 new FileInfo("C:\\Data\\flj.exe")
            };

            var result = _locationHelper.AreAllRequiredFilesPresent(local, REQUIRED_FILES);

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public void AreAllRequiredFilesPresent_ManyLocalFiles_ReturnsSuccess()
        {
            var local = new FileInfo[]
            {
                new FileInfo("C:\\Data\\ble.exe"),
                new FileInfo("C:\\Data\\flj.exe"),
                new FileInfo("C:\\Data\\ena.exe"),
                new FileInfo("C:\\Data\\dve.exe"),
                new FileInfo("C:\\Data\\tri.exe")
            };

            var result = _locationHelper.AreAllRequiredFilesPresent(local, REQUIRED_FILES);

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public void AreAllRequiredFilesPresent_OneFilesIsMissing_ReturnsFailure()
        {
            var local = new FileInfo[]
            {
                 new FileInfo("C:\\Data\\ble.exe")
            };

            var result = _locationHelper.AreAllRequiredFilesPresent(local, REQUIRED_FILES);

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public void AreAllRequiredFilesPresent_NoLocalFiles_ReturnsFailure()
        {
            var local = new FileInfo[]
            {

            };

            var result = _locationHelper.AreAllRequiredFilesPresent(local, REQUIRED_FILES);

            Assert.That(result.Result, Is.False);
        }
    }
}
