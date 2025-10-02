using common_ops.diagnostics.Constants;
using common_ops.Executors.Shell;
using System;
using System.Threading.Tasks;

namespace common_ops.Next.Executable
{
    internal class NextExecutableKiller : IExecutable
    {
        private IShellExecutor _shellExecutor;
        private readonly int[] _ports;

        public NextExecutableKiller(ShellExecutor shellExecutor, params int[] ports)
        {
            _shellExecutor = shellExecutor;
            _ports = ports.Length == 0 ? BiroNextConstants.NextPorts : ports;
        }

        /// <summary>
        /// Will check if next is running by looking at ports. If any of BiroNext ports are open it will attempt to:
        /// stop next modules (proxy_blobal, identity_server...), close ports (safetycheck) and stop runner global
        /// </summary>
        /// <returns></returns>
        public async Task<(bool Result, string Message)> Execute()
        {
            try
            {
                var result = string.Empty;

                result = await _shellExecutor.Kill_DotNetProcess_ByFullNameAsync("NextGlobal");
                await _shellExecutor.Kill_Process_ByTcpPortListenedAsync(_ports);
                var result2 = await _shellExecutor.Kill_Process_ByNameAsync("runner_global");

                result = string.IsNullOrEmpty(result) ? result2 : (result + Environment.NewLine + result2);
                result = result.StartsWith("No matching processes", StringComparison.CurrentCultureIgnoreCase) ? "Next was not running" : result;
                return (true, result);
            }
            catch (Exception ex)
            {
                return (true, ex.Message);
            }
        }
    }
}
