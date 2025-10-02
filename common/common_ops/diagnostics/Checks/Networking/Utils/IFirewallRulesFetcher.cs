using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Networking.Utils
{
    public interface IFirewallRulesFetcher
    {
        Task<string> Fetch();
    }
}