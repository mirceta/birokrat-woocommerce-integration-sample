using BirokratNext;
using BirokratNext.Exceptions;
using Newtonsoft.Json;
using si.birokrat.next.common.networking;
using si.birokrat.next.common.processing;
using si.birokrat.next.common.shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace tests.tools.fixture_setup
{
    public interface IBironextDriver {
        void Kill();
        void Start(string local_server);
        Task<bool> Validate(bool exc = false);
    }
    
    public class BironextDriver : IBironextDriver {

        static string bironextDeploymentPath = @"C:\Users\vucko\Desktop\playground\birokrat\bironext\server\deploy_global";
        int OpenProcessPid = -1;
        string pingApiKey;
        
        public BironextDriver(string deploymentPath, string pingApiKey) {
            bironextDeploymentPath = deploymentPath;
            this.pingApiKey = pingApiKey;
        }

        public async Task<bool> Validate(bool exc = false) {

            string bironextAddress = "http://localhost:19000/api/"; // THIS CLASS IS MEANT ONLY FOR LOCALHOST!

            if (!Directory.Exists(bironextDeploymentPath))
                throw new Exception($"bironext deployment path {bironextDeploymentPath} does not exist");

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(bironextAddress);
            client.Timeout = new TimeSpan(0, 0, 5);
            try {
                await client.GetAsync("test");
            } catch (Exception ex) {
                if (exc)
                    throw ex;
                return false;
            }

            BironextDeployment dep = new BironextDeployment();
            string result = await dep.Ping(bironextAddress, pingApiKey);
            if (!string.IsNullOrEmpty(result)) {
                if (exc)
                    throw new Exception($"Bironext is inaccessible!" + result);
                return false;
            } else {
                return true;
            }
        }

        public void Start(string local_server) {
            PrepareLoggerSettings(local_server);
            PrepareProxyGlobalSettings(local_server.Replace("\\", "\\\\"));
            PrepareIdentitySettings(local_server.Replace("\\", "\\\\"));

            string runnerPath = Path.Combine(bironextDeploymentPath, "runner_global", "bin", "Release");
            string result = si.birokrat.next.common.shell.PowerShell.ExecuteAndReturnResult($"cd {runnerPath}; $a = (Start-Process .\\runner_global.exe -passthru).Id; echo $a", true);
            try {
                OpenProcessPid = int.Parse(result.Trim());
                File.WriteAllText("openprocpid.txt", OpenProcessPid + "");
            } catch (Exception ex) {
                Console.WriteLine(result);
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                Environment.Exit(-1);
            }
        }

        public void Kill() {
            if (File.Exists("openprocpid.txt")) {
                string tmp = File.ReadAllText("openprocpid.txt");
                OpenProcessPid = int.Parse(tmp);
                ProcessUtils.KillbyPID(OpenProcessPid, true);
            } else {
                Console.WriteLine("WARNING RUNNER GLOBAL NOT KILLED!");
            }
            KillRelevantProcesses();

            string some = PowerShell.ExecuteAndReturnResult("Get-Process | Where-Object { $_.ProcessName -like \"'*Birokrat*'\" } | Stop-Process -Force\r\n", true);
            if (!string.IsNullOrWhiteSpace(some)) {
                throw new Exception("Unable to kill birokrats using powershell " + some);
            }
        }

        private void KillRelevantProcesses() {
            // WARNING: ASSUMES THAT PORTS OF BIRONEXT SERVICES NEVER CHANGE!
            int[] ports = { 19000, 19001, 5000, 19002, 19005 };
            foreach (int port in ports) {
                var pid = NetworkingUtils.FindProcessPIDByListeningTCPPort(port);
                if (pid != 0 && pid != -1)
                    ProcessUtils.KillbyPID(pid, true);
            }
        }

        private void PrepareIdentitySettings(string local_server) {
            string content;
            string identitySettingsBakPath = Path.Combine(bironextDeploymentPath, "identity_server", "bakappsettings.Secrets.json");
            string identitySettingsPath = Path.Combine(bironextDeploymentPath, "identity_server", "appsettings.Secrets.json");
            content = File.ReadAllText(identitySettingsBakPath);

            content = content.Replace("\"localhost\"", $"\"{local_server}\"");
            content = content.Replace("\"VMBIROBAZURE\"", $"\"{local_server}\"");

            File.WriteAllText(identitySettingsPath, content);
        }

        private void PrepareProxyGlobalSettings(string local_server) {
            string content;
            string proxySettingsBak = Path.Combine(bironextDeploymentPath, "proxy_global", "bakappsettings.Secrets.json");
            string proxySettings = Path.Combine(bironextDeploymentPath, "proxy_global", "appsettings.Secrets.json");
            content = File.ReadAllText(proxySettingsBak);

            content = content.Replace("\"localhost\"", $"\"{local_server}\"");

            File.WriteAllText(proxySettings, content);
        }

        private void PrepareLoggerSettings(string local_server) {
            string settingsBat = Path.Combine(bironextDeploymentPath, "appsettings.json");
            string content = File.ReadAllText(settingsBat);

            content = content.Replace("\"localhost\"", $"\"{local_server}\"");

            File.WriteAllText(settingsBat, content);
        }
    }

    public class BironextDeployment {

        const int TIMEOUT = 10;

        public BironextDeployment() { }
        public async Task<string> Ping(string bironextAddress, string bironextApiKey) {
            int retryCount = 0;
            while (true) {
                try {
                    var apiClient = new ApiClientV2(
                        apiAddress: bironextAddress,
                        apiKey: bironextApiKey,
                        timeoutSeconds: 60
                        );
                    
                    await apiClient.Logout();
                    var parameters = await apiClient.cumulative.Parametri("sifranti/artikli/stanjezaloge");
                    if (parameters.Count < 1) {
                        return JsonConvert.SerializeObject(parameters);
                    }

                    return null;
                } catch (ConcurrentRequestsNotAllowedException ex) {
                    retryCount++;
                    if (retryCount > 5)
                        return ex.ToString();
                    await Task.Delay(5000);
                } catch (Exception ex) {
                    return ex.ToString();
                }
            }
        }
    }
}
