using common_ops.diagnostics.Constants;
using common_ops.Executors.Shell;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Networking.Checks
{
    /// <summary>
    /// Represents a check and optional repair operation to ensure that a specified domain
    /// is correctly trusted under Windows Internet Settings (ZoneMap\Domains) for execution permissions.
    /// <para>
    /// This check verifies if the domain is registered under the Local Intranet zone (ZoneId = 1).
    /// If the domain is missing and repair is enabled, the class attempts to add the required registry entry.
    /// </para>
    /// <para>
    /// The operation executes PowerShell commands asynchronously through an injected <see cref="IShellExecutor"/>.
    /// Results are returned as <see cref="ResultRecord"/> instances, containing success status and detailed messages.
    /// </para>
    /// <para>
    /// Typical usage involves first checking whether the execution permission exists, and if necessary,
    /// automatically repairing it to prevent Windows "Open File - Security Warning" prompts when running files from network shares.
    /// </para>
    /// <para>
    /// <see cref="ResultRecord.AdditionalInfo"/> will contain lines for each operation (check and repair). For final result only
    /// look at <see cref="ResultRecord.Result"/>.
    /// </para>
    /// </summary>

    public class ZoneIdentifier_ExecutionPermission_CheckAndRepair : ICheck
    {
        IShellExecutor _shell;
        private readonly string _domainName;
        private readonly bool _doRepair;

        /// <summary>
        /// <inheritdoc cref="ZoneIdentifier_ExecutionPermission_CheckAndRepair"/>
        /// </summary>
        public ZoneIdentifier_ExecutionPermission_CheckAndRepair(IShellExecutor shell, string domainName, bool doRepair = false)
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
            bool result = false;

            var res1 = await _shell.ExecuteInBackgroundAsync(BuildCheckCommand(_domainName));
            info.Add(res1.Trim());

            if (info[0].Contains(TextConstants.POSTFIX_OK))
            {
                result = true;
            }

            if (!result && _doRepair)
            {
                var res2 = await _shell.ExecuteInBackgroundAsync(BuildRepairCommand(_domainName));
                info.Add(res2.Trim());

                if (info[1].Contains(TextConstants.POSTFIX_OK))
                    result = true;
            }

            return new ResultRecord(result, GetType().Name, info.ToArray());
        }

        private string BuildRepairCommand(string domainName)
        {
            return $@"
            $keyPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\{domainName}'

            try {{
                if (-not (Test-Path $keyPath)) {{
                    New-Item -Path $keyPath -Force | Out-Null
                }}
                
                # Set the '*' subkey to ZoneId 1 (Local Intranet)
                Set-ItemProperty -Path $keyPath -Name ""*"" -Value 1
                Write-Host ""ZoneMap internet rule added for '{domainName}'. {TextConstants.POSTFIX_OK}""
            }} catch {{
                Write-Host ""Could not add ZoneMap internet rule for '{domainName}'! {TextConstants.POSTFIX_WARNING}""
            }}";
        }

        private string BuildCheckCommand(string domainName)
        {
            return $@"
            $keyPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\{domainName}'

            if (-not (Test-Path $keyPath)) {{
                Write-Host ""'{domainName}' not found in internet settings ZoneMaps! {TextConstants.POSTFIX_WARNING}""
            }}
            else {{
                Write-Host ""'{domainName}' found in internet settings ZoneMaps! {TextConstants.POSTFIX_OK}""
            }}";
        }
    }
}
