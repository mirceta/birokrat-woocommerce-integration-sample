using System.Linq;
using System.Text;

namespace common_ops.Executors.Shell
{
    internal static class ShellCommands
    {
        internal static readonly char SHELL_SEPARATOR = (char)31; // ASCII 31 - Unit Separator <<<>>> 

        internal static readonly string ExecutionPolicySafetyCheck = "-NoProfile Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process ";

        internal static string Get_KillByTcpPorts(params int[] ports)
        {
            string portLine = $"$ports = @({string.Join(", ", ports)});";

            string command = @"
            " + ExecutionPolicySafetyCheck + @"
            " + portLine + @"
            $found = $false
            $processes = Get-NetTCPConnection | Where-Object { $ports -contains $_.LocalPort };
            foreach ($proc in $processes) {
            try {
                $processDetail = Get-Process -Id $proc.OwningProcess;
                if ($processDetail) {
                    $processDetail | Stop-Process -Force;
                    Write-Host 'Process terminated successfully.'
                } else {
                    Write-Host ""Process ID $($proc.OwningProcess) not found""
                }
                $found = $true
                } catch {
                    Write-Host ""Failed to terminate process ID $($proc.OwningProcess): $($_.Exception.Message)""
                }
            }
            if (-not $found) {
                Write-Host 'No processes found on the specified ports to terminate.'
            }";

            return command.Replace("\"", "\\\"");
        }

        internal static string Get_KillDotNetProcessByFullProcessNameFilter(string processName)
        {
            string command = @"
            " + ExecutionPolicySafetyCheck + @"
            try {
                $processes = Get-WmiObject Win32_Process -Filter ""Name = 'dotnet.exe'""
                $found = $false
                foreach ($process in $processes) {
                $ownerInfo = $process.GetOwner().User
                    if ($process.CommandLine -and $process.CommandLine -like '*" + processName + @"*') {
                        $found = $true
                        try {
                            $cml = $process.CommandLine
                            $proc = Get-Process -Id $process.ProcessId
                            $proc.Kill()
                            Write-Host ""Process $cml terminated successfully.""
                        } catch {
                            Write-Host ""Failed to terminate process: $($_.Exception.Message)""
                        }
                    }
                }
                if (-not $found) {
                    Write-Host 'No matching processes found to terminate.'
                }
            } catch {
                Write-Host ""Failed to execute script: $($_.Exception.Message)""
            }";
            return command.Replace("\"", "\\\"");
        }

        internal static string Get_KillProcessesByName(params string[] args)
        {
            var like = args.Where(x => !x.StartsWith("!")).Distinct().Select(x => $"$_.ProcessName -like '*{x}*'").ToArray();
            var notLike = args.Where(x => x.StartsWith("!")).Distinct().Select(x => $"$_.ProcessName -notlike '*{x.Replace("!", string.Empty)}*'").ToArray();

            if (like.Length == 0)
                return "Write-Host \"At least one parameter needs to be provided and at least one parameter needs to be without '!'\"";

            var executingCommand = new StringBuilder();
            if (like.Length > 0)
                executingCommand.Append($"({string.Join(" -or ", like)})");
            if (notLike.Length > 0)
                executingCommand.Append($" -and ({string.Join(" -and ", notLike)})");

            string command = @"
            " + ExecutionPolicySafetyCheck + @"
            try {
                $processes = Get-WmiObject Win32_Process | Where-Object {
                    try {
                        " + executingCommand + @"
                    } catch {
                        $false
                    }
                }
                $found = $false
                foreach ($process in $processes) {
                    $found = $true
                    try {
                        $proc = Get-Process -Id $process.ProcessId
                        $procName = $proc.Name
                        $proc.Kill()
                        Write-Host ""Process with name $procName terminated successfully.""
                    } catch {
                        Write-Host ""Failed to terminate process: $($_.Exception.Message)""
                    }
                }
                if (-not $found) {
                    Write-Host 'No matching processes found to terminate.'
                }
            } catch {
                Write-Host ""Failed to execute script: $($_.Exception.Message)""
            }";
            return command.Replace("\"", "\\\"");
        }

        /// <summary>
        /// returns requested ports then opened ports
        /// </summary>
        /// <param name="ports"></param>
        /// <returns></returns>
        internal static string Get_TcpPortsStatus(int[] ports)
        {
            string portLine = $"$ports = @({string.Join(", ", ports)});";

            string command = @"
                " + ExecutionPolicySafetyCheck + @"
                " + portLine + @"
                $openPorts = @()
                $processes = Get-NetTCPConnection | Where-Object { $ports -contains $_.LocalPort }
                
                if ($processes) {
                    $openPorts = $processes.LocalPort | Sort-Object -Unique
                    Write-Host ""$($ports -join ', ')"" 
                    Write-Host ""$($openPorts -join ', ')"" 
                    $missingPorts = $ports | Where-Object { -not ($openPorts -contains $_) }
                    if ($missingPorts) {
                        Write-Host ""The following requested ports are not open: $($missingPorts -join ', ')"" 
                    }
                } else {
                    Write-Host ""No specified ports are currently open.""
                }";
            return command.Replace("\"", "\\\"");
        }

        /// <summary>
        /// Returns firewall information in order: Rule name, direction, protocol, port. Separated with SHELL_SEPARATOR
        /// </summary>
        /// <returns></returns>
        internal static string GetFirewallRulesInfo()
        {
            return $@"Get-NetFirewallRule | Where-Object {{ $_.Enabled -eq 'True' }} | ForEach-Object {{
                $rule = $_
                $portFilters = Get-NetFirewallPortFilter -AssociatedNetFirewallRule $rule

                foreach ($pf in $portFilters) {{
                    $name = $rule.DisplayName
                    $direction = $rule.Direction
                    $protocol = $pf.Protocol
                    $port = $pf.LocalPort
                    
                    Write-Host ""$name{SHELL_SEPARATOR}$direction{SHELL_SEPARATOR}$protocol{SHELL_SEPARATOR}$port""
                }} }}";
        }

        internal static string GetSetFirewallRuleCommand(string name, string protocol, int port, string direction)
        {
            return $@"
            New-NetFirewallRule -DisplayName '{name}' -Direction {direction} -LocalPort {port} -Protocol {protocol.ToUpper()} -Action Allow -Profile Any
            if ($?) {{
                Write-Host ""SUCCESS""
            }} else {{
                Write-Host ""FAILED""
            }}";
        }
    }
}
