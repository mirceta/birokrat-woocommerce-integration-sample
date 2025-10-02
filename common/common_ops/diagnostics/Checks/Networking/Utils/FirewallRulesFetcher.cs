using common_ops.Executors.Shell;
using System.Threading.Tasks;
using System;

namespace common_ops.diagnostics.Checks.Networking.Utils
{
    public class FirewallRulesFetcher : IFirewallRulesFetcher
    {
        private readonly IShellExecutor _shellExecutor;
        private string _rules;
        private readonly int DELAY = 120;

        public FirewallRulesFetcher(IShellExecutor shellExecutor)
        {
            _shellExecutor = shellExecutor;
        }

        //public async Task<string> Fetch()   // with timeout
        //{
        //    if (!string.IsNullOrEmpty(_rules))
        //        return _rules;

        //    var firewallTask = _shellExecutor.ExecuteInBackgroundAsync(ShellCommands.GetFirewallRulesInfo(), true);

        //    if (await Task.WhenAny(firewallTask, Task.Delay(TimeSpan.FromSeconds(DELAY))) == firewallTask)
        //    {
        //        _rules = firewallTask.Result;
        //        return _rules;
        //    }
        //    else
        //    {
        //        throw new TimeoutException($"Fetching firewall rules took too long (timeout after {DELAY} seconds).");
        //    }
        //}

        public async Task<string> Fetch()
        {
            if (!string.IsNullOrEmpty(_rules))
                return _rules;

            var firewallTask = await _shellExecutor.ExecuteInBackgroundAsync(ShellCommands.GetFirewallRulesInfo(), true);
            return firewallTask;
        }
    }
}
