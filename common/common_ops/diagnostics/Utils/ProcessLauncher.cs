using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Utils
{
    public class ProcessLauncher : IProcessLauncher
    {
        public async Task<string> Start_DetachedProcessAsync(string exePath, bool asAdmin = true, bool hideWindow = false, string arguments = "")
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"\"{arguments}\"",
                UseShellExecute = true,
                CreateNoWindow = hideWindow,
                Verb = asAdmin ? "runAs" : string.Empty,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            };

            try
            {
                Process process = Process.Start(startInfo);
                await Task.Delay(1000);

                if (process == null)
                    return "Failed to start process.";
                else
                    return $"Process started with ID: {process.Id}";
            }
            catch (Exception ex)
            {
                return $"Failed to start process: {ex.Message}";
            }
        }

        public (bool Result, string Message) CanProcessRunAsAdminTest(string exePath)
        {
            try
            {
                if (!File.Exists(exePath))
                    return (false, "File does not exist. Provided path not valid");

                var processInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    process?.Kill();
                }

                return (true, "Can be run as admin.");
            }
            catch (UnauthorizedAccessException)
            {
                return (false, "UnauthorizedAccessException: The executable requires admin rights.");
            }
            catch (InvalidOperationException)
            {
                return (false, "InvalidOperationException: The file is not valid for execution.");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1223)
                {
                    return (true, "UAC prompt was canceled. App can be run with elevated privileges");
                }
                else
                {
                    return (false, "Win32Exception: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message);
            }
        }
    }
}
