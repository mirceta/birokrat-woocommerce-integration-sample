namespace common_ops.diagnostics.Constants
{
    public class BiroNextConstants
    {
        public static readonly int[] NextPorts = new int[] { 5000, 19000, 19001, 19002, 19005 };

        public static readonly string NextSettingsFileName = "appsettings.json";
        public static readonly string NextSettingsSecretsFileName = "appsettings.Secrets.json";

        public static readonly string[] RequiredBirokratDlls = new string[]
        {
            "CodeListConverter",
            "libFurs",
            "BiroMoneta",
            "libBiroRTC",
            "BiroNetUtils",
            "BiroNetUtils48",
            "zxing.interop"
        };
    }
}
