using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Environment.Utils
{
    public interface IInstalledFrameworkReader
    {
        Task<string> FetchSDKs();
        Task<string> FetchRuntimes();
    }
}
