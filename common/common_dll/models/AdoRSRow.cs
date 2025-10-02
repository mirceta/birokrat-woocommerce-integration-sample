using System.Collections.Generic;

namespace si.birokrat.next.common_dll.models {
    public class AdoRSRow {
        public List<AdoRSColumn> Columns { get; set; } = new List<AdoRSColumn>();
    }
}
