using common_ops.diagnostics.Checks.Location.Checks;
using common_ops.diagnostics.Checks.Location.Utils;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace common_ops_tests.LocationChecksTests
{
    [TestFixture]
    public class Sqlbirokrat_AccessAndPermission_Check_Test
    {
        Sqlbirokrat_AccessAndPermission_Check _check;
        Mock<ILocationHelper> _locationHelperMock;
        string _location;

        [SetUp]
        public void SetUp()
        {
            _location = "\\\\sqlbirokratlocation\\somelocation";
            _locationHelperMock = new Mock<ILocationHelper>();

            _check = new Sqlbirokrat_AccessAndPermission_Check(_locationHelperMock.Object);
        }

        [Test]
        public async Task Run_CanWriteAndReadFromSqlBirokrat_ReturnsSuccess()
        {
            _locationHelperMock.Setup(x => x.IsWritePermissionGranted(It.IsAny<string>()))
                .Returns(true);

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_ReadingAndWritingFromSqlBirokratDenied_ReturnsFailure()
        {
            _locationHelperMock.Setup(x => x.IsWritePermissionGranted(It.IsAny<string>()))
                .Returns(false);

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }
    }
}
