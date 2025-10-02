using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace common_ops.Executors.Shell
{
    public class ShellExecutor : IShellExecutor
    {
        public async Task<string> ExecuteInBackgroundAsync(string command, bool asAdmin = true)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = command,
                Verb = asAdmin ? "runAs" : string.Empty,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            StringBuilder sb = new StringBuilder();

            using (Process process = new Process())
            {
                process.StartInfo = info;
                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        sb.AppendLine(args.Data);
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        sb.AppendLine(args.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await WaitForExitAsync(process);
            }

            return sb.ToString();
        }

        private Task WaitForExitAsync(Process process)
        {
            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;

            process.Exited += (sender, args) =>
            {
                tcs.TrySetResult(true);
            };

            return tcs.Task;
        }

        #region SIMPLE EXECUTORS
        public void Execute(string command, string workingDirectory = null, bool visible = true, bool asAdmin = true)
        {
            var finalCommand = new StringBuilder();
            finalCommand.Append(command);
            finalCommand.Append(!visible ? "-WindowStyle Hidden" : string.Empty);
            finalCommand.Append(!asAdmin ? "-Verb RunAs" : string.Empty);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{finalCommand.ToString()}\"",
                UseShellExecute = visible,
                CreateNoWindow = !visible,
            };

            if (!string.IsNullOrEmpty(workingDirectory))
                startInfo.WorkingDirectory = workingDirectory;

            Process process = Process.Start(startInfo);
        }

        public void ExecuteExe(string fullName, string workingDirectory = "", bool asAdmin = true, string arguments = "")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fullName,
                Arguments = arguments,
                CreateNoWindow = false,
                Verb = asAdmin ? "RunAs" : "",
                UseShellExecute = fullName.Trim().EndsWith(".lnk", StringComparison.OrdinalIgnoreCase), //needed for shortcuts
                WorkingDirectory = string.IsNullOrEmpty(workingDirectory) ? Path.GetDirectoryName(fullName) : workingDirectory,
            };

            Process process = Process.Start(startInfo);
        }
        #endregion

        #region PROCESS KILLING

        public async Task<string> Kill_Next_Full(string rootFolderName, int[] ports, params string[] filter)
        {
            var result1 = await Kill_DotNetProcess_ByFullNameAsync(rootFolderName);
            await Kill_Process_ByTcpPortListenedAsync(ports);
            var result2 = await Kill_Process_ByNameAsync(filter);
            return result1 + Environment.NewLine + result2;
        }

        public async Task<string> Kill_Process_ByNameAsync(params string[] args)
        {
            var command = ShellCommands.Get_KillProcessesByName(args);
            return await ExecuteInBackgroundAsync(command, true);
        }

        public async Task<string> Kill_DotNetProcess_ByFullNameAsync(string filter)
        {
            var command = ShellCommands.Get_KillDotNetProcessByFullProcessNameFilter(filter);
            return await ExecuteInBackgroundAsync(command, true);
        }
        public async Task<string> Kill_Process_ByTcpPortListenedAsync(params int[] ports)
        {
            var command = ShellCommands.Get_KillByTcpPorts(ports);
            return await ExecuteInBackgroundAsync(command, true);
        }
        #endregion

        #region INFO GATHERING

        public async Task<string> Get_TCPPorts_ListOpenedAsync(params int[] ports)
        {
            var command = ShellCommands.Get_TcpPortsStatus(ports);
            return await ExecuteInBackgroundAsync(command, true);
        }
        #endregion
    }
}
