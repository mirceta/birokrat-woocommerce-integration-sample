using common_ops.diagnostics.Checks.Environment.Checks;
using common_ops.diagnostics.Checks.Environment.Utils;
using common_ops.diagnostics.Constants;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.EnvironmentChecksTests
{
    [TestFixture]
    public class Sdk_DotnetVersions_Check_Tests
    {
        Sdk_DotnetVersions_Check _check;
        Mock<IInstalledFrameworkReader> _dotnetVersionReaderMock;

        [SetUp]
        public void SetUp()
        {
            _dotnetVersionReaderMock = new Mock<IInstalledFrameworkReader>();
        }

        private Sdk_DotnetVersions_Check BuildCheck(params string[] versions)
        {
            return new Sdk_DotnetVersions_Check(_dotnetVersionReaderMock.Object, versions);
        }

        [Test]
        public async Task Run_AllDotNetVersionsInstalled_ReturnsSuccess()
        {
            _dotnetVersionReaderMock.Setup(x => x.FetchSDKs())
                .ReturnsAsync("2.1.202 [C:\\Program Files\\dotnet\\sdk]\r\n3.1.426 [C:\\Program Files\\dotnet\\sdk]\r\n8.0.403 [C:\\Program Files\\dotnet\\sdk]");

            var result = await BuildCheck("2.1", "3.1", "8.0").Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_AllDotNetVersionsNotInstalled_ReturnsWarning()
        {
            _dotnetVersionReaderMock.Setup(x => x.FetchSDKs())
                .ReturnsAsync("2.1.202 [C:\\Program Files\\dotnet\\sdk]\r\n3.1.426 [C:\\Program Files\\dotnet\\sdk]");

            var result = await BuildCheck("2.1", "3.1", "8.0").Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo.Any(x => x.EndsWith(TextConstants.POSTFIX_WARNING, System.StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public async Task Run_DotNetIsNotInstalled_ReturnsWarning()
        {
            _dotnetVersionReaderMock.Setup(x => x.FetchSDKs())
                .Throws(new System.Exception("DotNet not installed"));

            var result = await BuildCheck("2.1", "3.1", "8.0").Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo.Any(x => x.EndsWith(TextConstants.POSTFIX_WARNING, System.StringComparison.OrdinalIgnoreCase)), Is.True);
        }
    }
}
