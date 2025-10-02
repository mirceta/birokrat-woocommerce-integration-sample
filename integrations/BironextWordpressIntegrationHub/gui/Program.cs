using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gui_gen {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            
            /*
            Dictionary<string, object> parsedArgs = ParseArgs(args);

            string input_json = (string)parsedArgs["input_json_path"];
            string output_json = (string)parsedArgs["output_json_path"];
            bool is_test = (bool)parsedArgs["is_test"];
            */

            string input_json = "C:\\Users\\Administrator\\Desktop\\integrations_data\\input.json";
            string output_json = "C:\\Users\\Administrator\\Desktop\\integrations_data\\output.json";
            bool is_test = false;



            input_json = File.ReadAllText(input_json);
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new IntegrationObjectConfigRenderForm(input_json, output_json, is_test));
        }

        private static Dictionary<string, object> ParseArgs(string[] args) {
            Dictionary<string, object> parsedArgs = new Dictionary<string, object>();

            args.ToList().ForEach(x => {
                var parts = x.Split('=');
                if (parts[0] == "input_json_path") {
                    parsedArgs["input_json_path"] = parts[1];
                } else if (parts[0] == "output_json_path") {
                    parsedArgs["output_json_path"] = parts[1];
                } else if (parts[0] == "is_test") {
                    if (parts[1] != "true" && parts[1] != "false")
                        throw new Exception("is_test must be true or false!");
                    if (parts[1] == "true")
                        parsedArgs["is_test"] = true;
                    else
                        parsedArgs["is_test"] = false;
                }
            });

            //string datapath = @"C:\Users\Administrator\Desktop\integrations_data";

            string datapath = "C:\\Users\\Administrator\\Desktop\\integrations_data\\";
            if (!parsedArgs.ContainsKey("input_json_path")) {
                if (!File.Exists(Path.Combine(datapath, "input.json"))) throw new Exception("Args were empty, and default path was not found on this computer!");
                parsedArgs["input_json_path"] = Path.Combine(datapath, "input.json");
            } else if (!File.Exists((string)parsedArgs["input_json_path"])){
                throw new Exception("input_json_path does not exist on this computer!");
            }
            if (!parsedArgs.ContainsKey("output_json_path")) {
                if (!Directory.Exists(datapath)) throw new Exception("Args were empty, and default path was not found on this computer!");
                parsedArgs["output_json_path"] = Path.Combine(datapath, "output.json");
            } else if (!File.Exists((string)parsedArgs["output_json_path"])) {
                throw new Exception("output_json_path does not exist on this computer!");
            }

            if (!parsedArgs.ContainsKey("is_test"))
                parsedArgs["is_test"] = false;

            return parsedArgs;
        }
    }
}
