using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace common_ops.Executors.DotNet
{
    public class DotNetExecutor
    {
        public string BuildCommand(string dllLocation, string arguments, string extraArg = "")
        {
            var fullArgs = string.IsNullOrWhiteSpace(extraArg)
                ? arguments
                : $"{arguments} \"{extraArg}\"";

            return $"\"{dllLocation}\" {fullArgs}";
        }

        public async Task<string> ExecuteSimpleAsync(string command, string workingDir, bool asAdmin = true)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"exec {command}",
                WorkingDirectory = workingDir,
                Verb = asAdmin ? "runAs" : string.Empty,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            StringBuilder sb = new StringBuilder();

            using (Process process = new Process())
            {
                process.StartInfo = info;
                process.Start();

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
    }
}
