using common_ops.Executors.Shell;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops
{
    public class TaskMonitorHandler
    {
        private readonly IShellExecutor _shellExecutor;
        private readonly Action<string> _logger;
        private bool wasStopped;

        public TaskMonitorHandler(IShellExecutor shellExecutor, Action<string> logger)
        {
            _shellExecutor = shellExecutor;
            _logger = logger;
            wasStopped = false;
        }

        public async Task StartTaskMonitorAsync()
        {
            if (!wasStopped)
                return;

            try
            {
                var path = GetEasyScriptLauncherFullName();
                if (string.IsNullOrEmpty(path))
                    throw new FileNotFoundException();

                _shellExecutor.ExecuteExe(path);
                await Task.Delay(2000);
                _logger?.Invoke("TaskMonitor restarted");
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"TaskMonitor cant be started! Error: {ex.Message}");
            }
        }

        private string GetEasyScriptLauncherFullName()
        {
            var path = Directory.GetFiles(SystemPaths.GetCommonStartupFolderPath(), $"*EasyScriptLauncher*").FirstOrDefault();

            return string.IsNullOrEmpty(path) ? string.Empty : path;
        }

        public async Task StopTaskMonitorAsync()
        {
            string command = "Get-CimInstance Win32_Process -Filter \\\"Name = 'powershell.exe'\\\" | " +
                  "Where-Object { $_.CommandLine -like '*TaskMonitor*' } | " +
                  "ForEach-Object { " +
                  "try { " +
                  "Stop-Process -Id $_.ProcessId -Force -ErrorAction Stop; " +
                  "Write-Host \\\"Stopped process ID $($_.ProcessId)\\\" " +
                  "} catch { " +
                  "Write-Warning \\\"Failed to stop process ID $($_.ProcessId): $($_.Exception.Message)\\\" " +
                  "} }";

            var result = await _shellExecutor.ExecuteInBackgroundAsync(command, true);

            wasStopped = result.IndexOf("Stopped", StringComparison.OrdinalIgnoreCase) >= 0;

            if (wasStopped)
                _logger?.Invoke("TaskMonitor Stopped");
            else
                _logger?.Invoke("TaskMonitor was not running");
        }
    }
}
