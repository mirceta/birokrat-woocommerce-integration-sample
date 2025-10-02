using common_ops.diagnostics.Constants;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Environment.Utils
{
    public class InstalledFrameworkReader : IInstalledFrameworkReader
    {
        public async Task<string> FetchRuntimes()
        {
            return await FetchWithShell(
                "dotnet",
                "--list-runtimes",
                "The .NET CLI (dotnet) is not installed or not available in the system PATH. Please install it from https://dotnet.microsoft.com/download. ");
        }

        public async Task<string> FetchSDKs()
        {
            var standaloneSDKS = await FetchWithShell(
                "dotnet",
                "--list-sdks",
                "The .NET CLI (dotnet) is not installed or not available in the system PATH. Please install it from https://dotnet.microsoft.com/download. ");

            string psScript = @"
                $roots = @(
                  ""$env:ProgramFiles\dotnet\sdk"",
                  ""$env:ProgramFiles(x86)\dotnet\sdk"",
                  ""$env:ProgramFiles\Microsoft Visual Studio\Shared\dotnet\sdk"",
                  ""$env:ProgramFiles(x86)\Microsoft Visual Studio\Shared\dotnet\sdk""
                ) | Where-Object { Test-Path $_ }
                
                foreach ($r in $roots) {
                  Get-ChildItem -Path $r -Directory -ErrorAction SilentlyContinue |
                    Select-Object @{n='Name';e={$_.Name}}, @{n='FullName';e={$_.FullName}}
                }
                ";

            // Write script to a temp .ps1 file
            string tempPs1 = Path.Combine(Path.GetTempPath(), $"list_sdks_{Guid.NewGuid():N}.ps1");
            File.WriteAllText(tempPs1, psScript, Encoding.UTF8);

            try
            {
                var viaVisualStudio = await FetchWithShell(
                    "powershell.exe",
                    $"-NoProfile -ExecutionPolicy Bypass -File \"{tempPs1}\"",
                    "No SDK folders found under Visual Studio/shared dotnet roots.");

                return $"{standaloneSDKS}{System.Environment.NewLine}{viaVisualStudio}";
            }
            catch
            {
                return $"{standaloneSDKS}";
            }
            finally
            {
                try { File.Delete(tempPs1); } catch { }
            }
        }

        private async Task<string> FetchWithShell(string command, string argument, string exceptionMessage)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(command, argument)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                string output = string.Empty;
                using (Process process = Process.Start(processInfo))
                {
                    if (process == null)
                    {
                        throw new Exception(exceptionMessage + TextConstants.POSTFIX_ERROR);
                    }
                    output = await process.StandardOutput.ReadToEndAsync();
                    process.WaitForExit();
                    return output;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
