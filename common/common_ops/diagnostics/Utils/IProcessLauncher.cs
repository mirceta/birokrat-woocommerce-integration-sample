using System.Threading.Tasks;

namespace common_ops.diagnostics.Utils
{
    public interface IProcessLauncher
    {
        (bool Result, string Message) CanProcessRunAsAdminTest(string exePath);
        Task<string> Start_DetachedProcessAsync(string exePath, bool asAdmin = true, bool hideWindow = false, string arguments = "");
    }
}
