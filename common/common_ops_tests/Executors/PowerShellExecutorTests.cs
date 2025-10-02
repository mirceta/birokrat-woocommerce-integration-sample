using common_ops.Executors.Shell;
using NUnit.Framework;
using System.Threading.Tasks;

namespace common_ops_tests.Executors
{
    [TestFixture]
    public class PowerShellExecutorTests
    {
        IShellExecutor _executor;
        string _validCommand;

        [SetUp]
        public void SetUp()
        {
            _validCommand = @"Write-Host '123'";

            _executor = new ShellExecutor();
        }

        [Test]
        public async Task ExecuteInBackgroundAsync_TestExecution_ReturnsSuccess()
        {
            var result = await _executor.ExecuteInBackgroundAsync(_validCommand, true);

            Assert.That(result.Contains("123"), Is.True);
        }
    }
}
