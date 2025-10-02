using si.birokrat.next.common.logging;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace BiroWoocommerceHubTests
{
    class DaemonProcess
    {

        Process host;
        StringBuilder standardOutput;
        bool stdoutputlock = false;

        IMyLogger logger;
        public DaemonProcess(IMyLogger logger) {
            this.logger = logger;
        }

        public void Start(string command, bool administrator) {
            logger.LogInformation(command);
            ProcessStartInfo info = new ProcessStartInfo {
                FileName = "powershell",
                Arguments = $"/c \"{command}\"",
                Verb = administrator ? "runAs" : string.Empty,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.UTF8,
                StandardInputEncoding = Encoding.UTF8
            };

            standardOutput = new StringBuilder();
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.RedirectStandardInput = true;


            host = new Process() { StartInfo = info };
            host.OutputDataReceived += (sender, args) => {
                stdoutputlock = true;
                standardOutput.AppendLine(args.Data);
                stdoutputlock = false;
            };
            host.ErrorDataReceived += (sender, args) => {
                stdoutputlock = true;
                standardOutput.AppendLine(args.Data);
                stdoutputlock = false;
            };
            host.Start();
            host.BeginOutputReadLine();
            host.BeginErrorReadLine();

        }

        public void WriteStdin(string message) {
            host.StandardInput.WriteLineAsync(message);
        }

        public string ReadStdout() {
            while (stdoutputlock) { }
            return standardOutput.ToString();
        }

        public void WaitForExit() {
            host.WaitForExit();
        }

        public void Terminate() {
            host.StandardInput.WriteLine("EXIT");
            host.WaitForExit();
        }
    }
}
