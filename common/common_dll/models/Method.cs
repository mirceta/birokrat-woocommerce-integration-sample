namespace si.birokrat.next.common_dll.models {
    public class Method {
        public string Type { get; set; }

        public string Subtype { get; set; }

        public string Name { get; set; }

        public string Key { get; set; }


        // types
        public const string TYPE_INVITE = "INVITE";
        public const string TYPE_COLLECTFAILED = "COLLECTFAILED"; // after app failure or network down, client can collect the failed response
        public const string TYPE_NAVIGATION = "Navigation";
        public const string TYPE_CUMMULATIVE = "Cumulative";
        public const string TYPE_CODELIST = "CodeList";
        public const string TYPE_CREATEEDIT = "CreateEdit";
        public const string TYPE_POS = "POS";
        public const string TYPE_END = "End";
        public const string TYPE_IDSCANNING = "IdScanning";
    }

    
}
