using si.birokrat.next.common.build;
using si.birokrat.next.common.logging;
using si.birokrat.next.common.registration;
using si.birokrat.next.common.shell;
using si.birokrat.next.common_dll.models;
using si.birokrat.next.common_proxy_standard.models;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;

namespace si.birokrat.next.common_dll {
    public class Dll : IDisposable {

        public BirokratDllInterface Lib { get; private set; }
        private Process birokratProcess;
        private const string pot_do_datotek = null;

        public Dll(DllInfo info, string guid = null, bool procstart = true) {
            // here the instance of birokrat should be started with arguments...
            if (guid == null) 
                guid = Guid.NewGuid().ToString();
            Lib = new BirokratDllInterface(guid);
            if (procstart)
                StartBirokratProcess(info, guid, pot_do_datotek);
        }
        ~Dll() {
            Dispose();
        }

        public void Dispose() {
            if (Lib != null)
                Lib.Dispose();
            Lib = null;
            try {
                birokratProcess.Kill();
            } catch (Exception ex) { }
        }

        private void StartBirokratProcess(DllInfo info, string dll_id, string pot_do_datotek = null) {

            // get pot do datotek
            if (pot_do_datotek == null) {
                string key = Environment.Is64BitOperatingSystem ? @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Andersen\Birokrat" : @"HKEY_LOCAL_MACHINE\SOFTWARE\Andersen\Birokrat";
                pot_do_datotek = RegistryUtils.GetRegistryValue(key, "Pot");
            }

            string args = $"###{info.UserName}###{info.Password}###{info.PoslovnoLeto}###{info.SqlServer}###{info.Mode}###{info.TaxNumber}###{dll_id}###";

            //PowerShell.Execute($"{Path.Combine(pot_do_datotek, "Birokrat.exe")} {args}", true);

            birokratProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = Path.Combine(pot_do_datotek, "Birokrat.exe"),
                    Arguments = args,
                    Verb = "runAs",
                    //CreateNoWindow = false,
                    UseShellExecute = true,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false
                }
            };

            try {
                birokratProcess.Start();
            } catch (System.ComponentModel.Win32Exception ex) {
                Logger.Log("Exception", ex.Message + ex.StackTrace , toConsole: true);
                Console.ReadLine();
                Environment.Exit(-1);
            }

        }
    }
}
