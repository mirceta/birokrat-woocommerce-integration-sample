namespace si.birokrat.next.common_proxy_standard.models {
    public class DataInfo : Info {
        public int AccountId { get; set; } = 0;

        public int ApplicationId { get; set; } = 0;

        public string SqlUsername { get; set; } = string.Empty;

        public string YearCode { get; set; } = string.Empty;
    }
}
