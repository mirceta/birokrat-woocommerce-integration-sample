using si.birokrat.next.common.conversion;
using System;
using System.Data;

namespace si.birokrat.next.common_mvc.querying {
    public static class PaginatedResultFactory {
        public static PaginatedResult<T> Create<T>(DataTable dataTable, int perPage, int page, string orderBy, string orderDirection, string filter) where T : new () {
            var response = new PaginatedResult<T>();

            if (dataTable.Rows.Count > 0) {
                response.PerPage = perPage;
                response.Page = page;
                response.From = (page - 1) * perPage + 1;
                response.To = response.From + dataTable.Rows.Count - 1;
                response.Total = (int)dataTable.Rows[0]["Total"];
                response.LastPage = perPage == -1 ? 1 : (int)Math.Ceiling((double)response.Total / perPage);
                response.Filter = filter;
                response.OrderBy = orderBy;
                response.OrderDirection = orderDirection;

                foreach (DataRow row in dataTable.Rows) {
                    T item = new T();
                    DatabaseConverter.ObjectFromDataRow(item, row);
                    response.Items.Add(item);
                }
            }

            return response;
        }
    }
}
