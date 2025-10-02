using si.birokrat.next.common.build;
using si.birokrat.next.common.shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace tests
{
    public class DatabaseOps
    {

        string scriptsPath = Path.Combine(Build.ProjectPath, "tools", "fixture_setup", "scripts");
        string customerScriptConfigsPath = Path.Combine(Build.ProjectPath, "database_configs");

        public DatabaseOps(string scriptsPath, string customerScriptConfigsPath) {
            this.scriptsPath = scriptsPath;
            this.customerScriptConfigsPath = customerScriptConfigsPath;
        }
        
        public string ResetDatabase(string customerSettingsFile, string localsql, string localbackuppath) {
            
            string customerSettingsPath = Path.Combine(customerScriptConfigsPath, customerSettingsFile);
            string content = File.ReadAllText(customerSettingsPath);

            content = content.Replace("[[[local_sql_server]]]", "'" + localsql + "'");
            content = content.Replace("[[[local_sql_backup_path]]]", "'" + localbackuppath + "'");

            string endSettingsFilePath = Path.Combine(scriptsPath, "settings.ps1");
            if (File.Exists(endSettingsFilePath)) {
                File.Delete(endSettingsFilePath);
            }
            File.WriteAllText(Path.Combine(scriptsPath, "settings.ps1"), content);
            string result = ExecuteSetupScript().ToString();
            if (!result.ToLower().Contains("true")) {
                throw new Exception("Database restoration failed!");
            }
            return null;
        }

        public void Validate(string customerSettingsFile, string localsql, string localbackuppath) {


            string filepath = $"{Path.Combine(customerScriptConfigsPath, customerSettingsFile)}_settings.ps1";
            if (!File.Exists(filepath)) {
                throw new Exception($"customer settings file {filepath} does not exist!");
            }
            if (!Directory.Exists(localbackuppath))
                throw new Exception($"local backup path {localbackuppath} does not exist!");
        }

        private bool ExecuteSetupScript() {
            string prev = Directory.GetCurrentDirectory();
            string some = "";
            try {
                Directory.SetCurrentDirectory(scriptsPath);
                string cmd = $@". .\procedures.ps1";
                some = PowerShell.ExecuteAndReturnResult(cmd, true);
                Console.WriteLine(some);
            } catch (Exception ex) { } finally {
                Directory.SetCurrentDirectory(prev);
            }
            return !some.Contains("NOT SAFE TO EXECUTE");
        }
    }
}
