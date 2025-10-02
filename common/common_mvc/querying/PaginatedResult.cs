using System.Collections.Generic;

namespace si.birokrat.next.common_mvc.querying {
    public class PaginatedResult<T> {
        public int PerPage { get; set; } = 0;

        public int Page { get; set; } = 0;

        public int From { get; set; } = 0;

        public int To { get; set; } = 0;

        public int Total { get; set; } = 0;

        public int LastPage { get; set; } = 0;

        public string Filter { get; set; }

        public string OrderBy { get; set; } = string.Empty;

        public string OrderDirection { get; set; } = string.Empty;

        public List<T> Items { get; set; } = new List<T>();
    }
}
