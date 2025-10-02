using common_ops.diagnostics.Constants;
using common_ops.Executors.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Networking.Checks
{
    /// <summary>
    /// Temporarily connects to a network share, identifies the user account associated with the connection,
    /// and ensures cleanup of any active session before and after the check. Will return user for the connection
    /// </para>
    /// <para>
    /// The operation executes PowerShell commands asynchronously through an injected <see cref="IShellExecutor"/>.
    /// Results are returned as <see cref="ResultRecord"/> instances, containing success status and detailed messages.
    /// </para>
    /// <para>
    /// <see cref="ResultRecord.AdditionalInfo"/> Result will depend if connection can be established. Additional info
    /// will contain username
    /// look at <see cref="ResultRecord.Result"/>.
    /// </para>
    /// </summary>

    public class UsedUser_InternalConnection_Check : ICheck
    {
        IShellExecutor _shell;
        private readonly string _domainName;
        private readonly bool _doRepair;

        /// <summary>
        /// <inheritdoc cref="UsedUser_InternalConnection_Check"/>
        /// </summary>
        public UsedUser_InternalConnection_Check(IShellExecutor shell, string domainName, bool doRepair = false)
        {
            _shell = shell;
            _domainName = domainName;
            _doRepair = doRepair;
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                return await Work();
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
        }

        private async Task<ResultRecord> Work()
        {
            List<string> info = new List<string>();

            TimeSpan delay = TimeSpan.FromSeconds(10);

            await _shell.ExecuteInBackgroundAsync(CloseConnection(_domainName));
            var output = await Fetch();
            await _shell.ExecuteInBackgroundAsync(CloseConnection(_domainName));

            var final = output.Replace('\r', ' ')
                .Split('\n')
                .Where(x => x.StartsWith(_domainName))
                .Select(x => x.Trim())
                .ToArray();

            var result = output.IndexOf(_domainName, StringComparison.OrdinalIgnoreCase) >= 0; 

            return new ResultRecord(result, GetType().Name, final);
        }

        public async Task<string> Fetch() 
        {
            var checkTask = _shell.ExecuteInBackgroundAsync(BuildCheckCommand(_domainName));

            if (await Task.WhenAny(checkTask, Task.Delay(TimeSpan.FromSeconds(10))) == checkTask)
            {
                return checkTask.Result;
            }
            else
            {
                throw new TimeoutException($"Couldn't establish the connection. Timeout!");
            }
        }


        private string CloseConnection(string domainName)
        {
            return $@"net use '{domainName}' /delete";
        }
        private string BuildCheckCommand(string domainName)
        {
            return $@"
            net use '{domainName}'
            wmic netuse get RemoteName,UserName";
        }
    }
}
