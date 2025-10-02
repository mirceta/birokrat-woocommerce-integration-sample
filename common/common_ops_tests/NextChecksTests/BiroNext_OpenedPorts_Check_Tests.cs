using common_ops.diagnostics.Checks.Next.Checks;
using common_ops.diagnostics.Constants;
using common_ops.Executors.Shell;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace common_ops_tests.NextChecksTests
{
    [TestFixture]
    public class BiroNext_OpenedPorts_Check_Tests
    {
        Mock<IShellExecutor> _shellExecutorMock;
        BiroNext_OpenedPorts_Check _check;
        int[] _ports;

        [SetUp]
        public void SetUp()
        {
            _ports = BiroNextConstants.NextPorts;
            _shellExecutorMock = new Mock<IShellExecutor>();
            _check = new BiroNext_OpenedPorts_Check(_shellExecutorMock.Object, _ports);
        }

        [Test]
        public async Task Run_AllPortsAreOpened_ReturnsSuccess()
        {
            _shellExecutorMock.Setup(x => x.Get_TCPPorts_ListOpenedAsync(_ports))
                .ReturnsAsync("5000, 19000, 19001, 19002, 19005\r\n5000, 19000, 19001, 19002, 19005\r\n");

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_AllPortsAreNotOpened_ReturnsFailure()
        {
            _shellExecutorMock.Setup(x => x.Get_TCPPorts_ListOpenedAsync(_ports))
                .ReturnsAsync("5000, 19000, 19001, 19002, 19005\r\n5000, 19001, 19002, 19005\r\nThe following requested ports are not open: 19000\r\n");

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_NoInfoReceived_ReturnsFailure()
        {
            _shellExecutorMock.Setup(x => x.Get_TCPPorts_ListOpenedAsync(_ports))
                .ReturnsAsync("");

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }
    }
}
