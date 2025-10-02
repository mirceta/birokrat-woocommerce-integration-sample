using System.IO;

namespace common_ops.diagnostics.Constants
{
    public static class BiroLocationConstants
    {
        public static readonly string SharedSettingsLocation;
        // Birokrat
        public static readonly string BirokratDLL_DefaultFolder = "C:\\Birokrat.DLL";
        public static readonly string[] BirokratDLL_RequredFolders = new string[]
        {
            "CodeListConverter",
            "FURS",
            "RTC",
            "Utils"
        };

        public static readonly string BirokratDefaultLocation = "C:\\Birokrat";
        public static readonly string BirokratExeFileName = "Birokrat.exe";
        public static readonly string BirokratExeBackupFolder = "C:\\Birokrat\\Birokrat_exe_Backup";
        // Next
        public static readonly string DefaultNextLocation = "C:\\Birokrat\\NextGlobal\\LATEST";
        public static readonly string[] NextFolders = new string[]
        {
            "api_core",
            "api_wrapper",
            "biro_instance_pool",
            "identity_server",
            "proxy_global",
            "runner_global"
        };

        // Next instalation
        public static readonly string SqlBirokratDeploymentsPath = "\\\\sqlbirokrat\\Birokrat ni za distribucijo\\Bironext\\delivery\\bironext_server";
        public static readonly string LocalRunnerGlobalPath = "runner_global\\bin\\Release\\runner_global.exe";
        public static readonly string BironextLocalArchivePath;
        public static readonly string BironextLocalLatestPath;
        public static readonly string BironextBasePath;
        public static readonly string BironextInstallInfoFile = "info.txt";
        // SqlBirokrat
        public static readonly string DefaultSqlbirokratLocation = "\\\\sqlbirokrat\\Andersen";
        public static readonly string DefaultDLLOrigin = "\\\\razvoj2016\\Birokrat.DLL";

        static BiroLocationConstants()
        {
            BironextBasePath = Path.Combine(BirokratDefaultLocation, "NextGlobal");
            BironextLocalArchivePath = Path.Combine(BironextBasePath, "archive");
            BironextLocalLatestPath = Path.Combine(BironextBasePath, "LATEST");
            SharedSettingsLocation = Path.Combine(BironextBasePath, "SharedSettings");
        }
    }
}
