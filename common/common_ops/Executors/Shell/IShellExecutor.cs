using System.Threading.Tasks;

namespace common_ops.Executors.Shell
{
    public interface IShellExecutor
    {
        Task<string> Get_TCPPorts_ListOpenedAsync(params int[] ports);
        Task<string> Kill_DotNetProcess_ByFullNameAsync(string filter);
        Task<string> Kill_Process_ByNameAsync(params string[] args);
        Task<string> Kill_Process_ByTcpPortListenedAsync(params int[] ports);
        void Execute(string command, string workingDirectory = null, bool visible = true, bool asAdmin = true);
        Task<string> ExecuteInBackgroundAsync(string command, bool asAdmin = true);
        void ExecuteExe(string fullName, string workingDirectory = "", bool asAdmin = true, string arguments = "");
    }
}
