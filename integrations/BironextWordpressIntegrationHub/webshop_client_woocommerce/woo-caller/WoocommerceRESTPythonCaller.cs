using common_python;
using si.birokrat.next.common.build;
using si.birokrat.next.common.logging;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using webshop_client_woocommerce.woo_caller;

namespace BiroWoocommerceHubTests
{

    public partial class WoocommerceRESTPythonCaller : IWooApiCaller {

        string tool_location = Path.Combine(Build.SolutionPath, @"woocommercepython\python_caller_v1.py");

        string address;
        string ck;
        string cs;
        string version;
        int tool_version;
        string tool_loc;

        public WoocommerceRESTPythonCaller(string address, string ck, string cs, string version, int tool_version = 1, string tool_loc = null) {
            this.address = address;
            this.ck = ck;
            this.cs = cs;
            this.version = version;
            this.tool_version = tool_version;
            this.tool_loc = tool_loc;

            if (!string.IsNullOrEmpty(tool_loc)) {
                tool_location  = Path.Combine(tool_loc, $@"woocommercepython\python_caller_v{tool_version}.py");
            } else {
                tool_location = Path.Combine(Build.SolutionPath, $@"woocommercepython\python_caller_v{tool_version}.py");
            }
            logger = new ConsoleMyLogger();

            if (!PythonInstallationChecker.IsPythonInstalled()) {
                throw new Exception("Python 3.8 is not installed and is required to run this program!");
            }

            string libraryName = "woocommerce"; // replace with the library you're checking
            bool isInstalled = PythonInstallationChecker.IsLibraryInstalled("python", libraryName);
            if (!isInstalled)
                throw new Exception("Python must have installed the woocommerce library. Run pip install woocommerce");

            if (!File.Exists(tool_location)) {
                throw new Exception($"File does not exist: {tool_location}");
            }
        }

        IMyLogger logger;
        public void SetLogger(IMyLogger logger) {
            this.logger = logger;
        }

        public string Ck { get => ck; }
        public string Cs { get => cs; }
        public string Address { get => address; }
        public string Version { get => version; }

        public async Task<string> Post(string op, string body) {
            string bodyfile = Path.Combine(new FileInfo(tool_location).Directory.FullName, $"{Guid.NewGuid().ToString()}body.json");
            File.WriteAllText(bodyfile, body);//, Encoding.UTF8 );
            return fixwoojson(await execute("post", op, bodyfile));
        }

        public async Task<string> Put(string op, string body) {
            string bodyfile = Path.Combine(new FileInfo(tool_location).Directory.FullName, $"{Guid.NewGuid().ToString()}body.json");
            File.WriteAllText(bodyfile, body);//, Encoding.UTF8 );
            return fixwoojson(await execute("put", op, bodyfile));
        }

        public async Task<string> Get(string op) {
            return fixwoojson(await execute("get", op));
        }

        public async Task<string> Delete(string op) {
            return fixwoojson(await execute("delete", op));
        }

        #region [auxiliary]
        private async Task<string> execute(string optype, string op, string bodyfile = null) {
            logger.LogInformation("EXECUTING");
            int fails = 0;
            while (true) {
                
                try {
                    DaemonProcess proc = new DaemonProcess(logger);
                    string line = commandline_string(optype, op, bodyfile);
                    proc.Start(line, true);
                    string result = "";
                    while (result == "") {
                        result = proc.ReadStdout();
                        await Task.Delay(100);
                    }
                    result = proc.ReadStdout();
                    if (string.IsNullOrEmpty(result.Trim())) {
                        throw new Exception("Result empty");
                    }
                    proc.Terminate();
                    return result;
                } catch (Exception ex) {
                    logger.LogInformation($"DAEMON EXECUTION ERROR.. RETRYING: {ex.Message + ex.StackTrace.ToString()}");
                    fails++;
                    if (fails > 10)
                    {
                        throw ex;
                    }
                }
            }
        }

        private string commandline_string(string httpop, string op, string body = null) {
            return $"python {tool_location} {address} {ck} {cs} {version} {httpop} {op}" + ((body == null) ? "" : $" --bodyfile {body}");
        }

        private static string fixwoojson(string json) {
            return json.Replace(": False", ": false").Replace(": True", ": true").Replace(": None", ": null");
        }
        #endregion
    }
}
