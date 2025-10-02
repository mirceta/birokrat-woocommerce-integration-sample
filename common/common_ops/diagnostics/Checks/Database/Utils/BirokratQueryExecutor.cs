using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.Linq;
using common_ops.Executors.Sql;

namespace common_ops.diagnostics.Checks.Database.Utils
{
    public class BirokratQueryExecutor : IBirokratQueryExecutor
    {
        private readonly IDatabaseQueryExecutor _queryExecutor;

        public BirokratQueryExecutor(IDatabaseQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor;
        }

        public async Task<List<string>> GetYearcodeDatabases_ThatMatchesTaxNumberAsync(string connectionString, string taxNumber = "")
        {
            var result = await _queryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(
                connectionString,
                SqlQueries.GetYearcodeDatabaseNames(taxNumber));

            var dbs = new List<string>();

            foreach (var item in result)
            {
                if (item.IndexOf("temp", StringComparison.CurrentCultureIgnoreCase) >= 0) //this works the same as: if (item.Contains("temp", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (Regex.IsMatch(item, taxNumber))
                    dbs.Add(item);
            }
            return dbs;
        }

        public async Task<List<string>> GetYearcodes_ThatMatchesTaxNumberAsync(string connectionString, string taxNumber = "")
        {
            var result = await GetYearcodeDatabases_ThatMatchesTaxNumberAsync(connectionString, taxNumber);
            var yearcodes = result.OrderBy(x => x).Select(x => x = x.Substring(x.IndexOf("-") + 1)).ToList();
            return yearcodes;
        }

        public async Task<List<string>> GetAllDatabases_ThatMatchesTaxNumberAsync(string connectionString, string taxNumber = "")
        {
            var result = await _queryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(
                connectionString,
                SqlQueries.GetDatabasesNames());

            var dbs = new List<string>();

            foreach (var item in result)
            {
                if (item.IndexOf("temp", StringComparison.CurrentCultureIgnoreCase) >= 0) //this works the same as: if (item.Contains("temp", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (Regex.IsMatch(item, taxNumber))
                    dbs.Add(item);
            }
            return dbs;
        }

        public async Task<List<string>> GetAllTaxNumbers_FromYearcodeDatabasesAsync(string connectionString)
        {
            var result = await _queryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(
                connectionString,
                SqlQueries.GetDatabasesNames());

            var taxNumbers = new List<string>();

            foreach (var item in result)
            {
                if (item.IndexOf("temp", StringComparison.CurrentCultureIgnoreCase) >= 0) //this works the same as: if (item.Contains("temp", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                foreach (Match match in Regex.Matches(item, @"\d{8}"))
                {
                    if (!taxNumbers.Contains(match.Value))
                        taxNumbers.Add(match.Value);
                }
            }
            return taxNumbers;
        }

        public async Task<List<string>> GetCoreDatabasesAsync(string connectionString)
        {
            var result1 = await _queryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(
                connectionString,
                SqlQueries.GetDatabasesNames());

            var dbs = new List<string>();

            foreach (var item in result1)
            {
                if (item.IndexOf("temp", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    continue;
                if (Regex.IsMatch(item, "^[a-zA-Z]+$"))
                    dbs.Add(item);
            }
            return dbs;
        }
    }
}
