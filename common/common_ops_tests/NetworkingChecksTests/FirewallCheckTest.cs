using common_ops.diagnostics.Checks.Networking.Checks;
using common_ops.diagnostics.Checks.Networking.Utils;
using common_ops.diagnostics.Constants;
using common_ops.Executors.Shell;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.NetworkingChecksTests
{
    [TestFixture]
    public class FirewallCheckTest
    {
        Firewall_PortsByName_CheckAndRepair _check;
        Mock<IShellExecutor> _executorMock;
        Mock<IFirewallRulesFetcher> _rulesFetcherMock;

        readonly string _name = "CUSTOM RULE";
        readonly FirewallRule_Direction _direction = FirewallRule_Direction.Outbound;
        readonly FirewallRule_Protocol _protocol = FirewallRule_Protocol.TCP;
        readonly int _port = 1919;

        [SetUp]
        public void SetUp()
        {
            _executorMock = new Mock<IShellExecutor>();
            _rulesFetcherMock = new Mock<IFirewallRulesFetcher>();

            _check = new Firewall_PortsByName_CheckAndRepair(_executorMock.Object, _rulesFetcherMock.Object, _name, _direction, _protocol, _port, true);
        }

        [Test]
        public async Task Run_RuleIsOk_ReturnsSuccess()
        {
            var line = $"Game Bar\u001fOutbound\u001fAny\u001fAny{Environment.NewLine}{_name}\u001f{_direction}\u001f{_protocol}\u001f{_port}";
            _executorMock.Setup(x => x.ExecuteInBackgroundAsync(It.IsAny<string>(), true))
                .ReturnsAsync(line);

            _rulesFetcherMock.Setup(x => x.Fetch())
                .ReturnsAsync(line);

            var checkResult = await _check.Run();

            Assert.That(checkResult.Result, Is.True);
            Assert.That(checkResult.AdditionalInfo.Any(x => x.EndsWith(TextConstants.POSTFIX_ERROR)), Is.False);
        }

        [Test]
        public async Task Run_NoRuleForPortWithRepairON_ReturnsSuccess()
        {
            _rulesFetcherMock.Setup(x => x.Fetch())
               .ReturnsAsync($"Game Bar\u001fOutbound\u001fAny\u001fAny");

            _executorMock.Setup(x => x.ExecuteInBackgroundAsync(It.IsAny<string>(), true))
                .ReturnsAsync("SUCCESS");

            var checkResult = await _check.Run();

            Assert.That(checkResult.Result, Is.True);
            Assert.That(checkResult.AdditionalInfo.Any(x => x.EndsWith(TextConstants.POSTFIX_REPAIR)), Is.True);
        }

        [Test]
        public async Task Run_DuplicateRules_ReturnsWarnings()
        {
            _rulesFetcherMock.Setup(x => x.Fetch())
               .ReturnsAsync($"{_name}1\u001f{_direction}\u001f{_protocol}\u001f{_port}{Environment.NewLine}{_name}\u001f{_direction}\u001f{_protocol}\u001f{_port}");

            var checkResult = await _check.Run();

            Assert.That(checkResult.Result, Is.True);
            Assert.That(checkResult.AdditionalInfo.Where(x => x.Contains(TextConstants.POSTFIX_WARNING)).Count() == 2, Is.True);
        }
    }
}
