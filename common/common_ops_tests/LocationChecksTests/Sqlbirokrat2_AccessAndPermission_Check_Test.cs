using common_ops.diagnostics.Checks.Location.Checks;
using common_ops.diagnostics.Checks.Location.Utils;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace common_ops_tests.LocationChecksTests
{
    [TestFixture]
    public class Sqlbirokrat2_AccessAndPermission_Check_Test
    {
        Sqlbirokrat2_AccessAndPermission_Check _check;
        Mock<ILocationHelper> _locationHelperMock;
        string _location;

        [SetUp]
        public void SetUp()
        {
            _location = "\\\\sqlbirokrat2location\\somelocation";
            _locationHelperMock = new Mock<ILocationHelper>();

            _check = new Sqlbirokrat2_AccessAndPermission_Check(_locationHelperMock.Object);
        }

        [Test]
        public async Task Run_CanReadFromSqlBirokrat2_ReturnsSuccess()
        {
            _locationHelperMock.Setup(x => x.IsReadPermissionGranted(It.IsAny<string>()))
                .Returns(true);

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_ReadingFromSqlBirokrat2Denied_ReturnsFailure()
        {
            _locationHelperMock.Setup(x => x.IsReadPermissionGranted(It.IsAny<string>()))
                .Returns(false);

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }
    }
}
