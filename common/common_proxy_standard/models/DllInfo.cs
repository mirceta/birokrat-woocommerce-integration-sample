namespace si.birokrat.next.common_proxy_standard.models {
    public class DllInfo : Info {
        public string Token { get; set; } = string.Empty;

        public string DeviceIdentifier { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string PoslovnoLeto { get; set; } = string.Empty;

        public string Mode { get; set; } = string.Empty;

        // DEPRECATED (LEFT FOR COMPATIBILITY REASONS)

        public int Fiscalization { get; set; } = 0;

        public string FilePath { get; set; } = string.Empty;

        public bool Global { get; set; } = false;

        public new string ToString() {
            string retval = "";
            if (!string.IsNullOrEmpty(Token)) {
                retval += $"Token: {Token}";
            }
            if (!string.IsNullOrEmpty(DeviceIdentifier)) {
                retval += $"DeviceIdentifier: {DeviceIdentifier}";
            }
            if (!string.IsNullOrEmpty(UserName)) {
                retval += $"UserName: {UserName}";
            }
            if (!string.IsNullOrEmpty(Password)) {
                retval += $"Password: {Password}";
            }
            if (!string.IsNullOrEmpty(PoslovnoLeto)) {
                retval += $"PoslovnoLeto: {PoslovnoLeto}";
            }
            if (!string.IsNullOrEmpty(Mode)) {
                retval += $"Mode: {Mode}";
            }
            // fiscalization
            retval += $"Fiscalization: {Fiscalization}";
            if (!string.IsNullOrEmpty(FilePath)) {
                retval += $"FilePath: {FilePath}";
            }
            // global
            retval += $"Global: {Global.ToString()}";
            return retval;
        }
    }
}
