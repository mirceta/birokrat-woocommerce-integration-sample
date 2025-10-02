using common_ops.diagnostics.Constants;
using common_ops.Executors.Shell;
using System;
using System.Threading.Tasks;

namespace common_ops.Next.Executable
{
    internal class BirokratExecutableKiller : IExecutable
    {
        private IShellExecutor _shellExecutor;

        public BirokratExecutableKiller(ShellExecutor shellExecutor)
        {
            _shellExecutor = shellExecutor;
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
                var result = await _shellExecutor.Kill_Process_ByNameAsync(BiroLocationConstants.BirokratExeFileName);
                return (true, result);
            }
            catch (Exception ex)
            {
                return (true, ex.Message);
            }
        }
    }
}
