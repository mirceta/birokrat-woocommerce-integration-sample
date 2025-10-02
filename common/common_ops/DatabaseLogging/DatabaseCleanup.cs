using common_ops.Executors.Sql;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace common_ops.DatabaseLogging
{
    internal class DatabaseCleanup
    {
        private readonly IDatabaseQueryExecutor _queryExecutor;
        private readonly string _connectionString;
        private readonly DatabaseName _databaseName;

        public DatabaseCleanup(IDatabaseQueryExecutor queryExecutor, string connectionString, DatabaseName databaseName)
        {
            _queryExecutor = queryExecutor;
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        public async Task RemoveOldDatabases()
        {
            var result = await _queryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(
               _connectionString,
               $"SELECT name FROM Sys.databases WHERE name LIKE '%{_databaseName.BaseName}%'");

            var dates = result.Select(x => ExtractDateFromDbName(x))
                .OrderByDescending(x => x.Date)
                .ToArray();

            for (int i = 2; i < dates.Length; i++)
            {
                await _queryExecutor.ExecuteNonQueryAsync(
                    _connectionString,
                    $"IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '{dates[i].DbName}') DROP DATABASE [{dates[i].DbName}];");
            }
        }

        private (DateTime Date, string DbName) ExtractDateFromDbName(string dbName)
        {
            // Matches ..._YYYY_MM at the end; accepts 1-2 digits for M
            var matches = Regex.Match(dbName, @"_(\d{4})[_-](\d{1,2})$");

            if (matches.Success
                && int.TryParse(matches.Groups[1].Value, out var y)
                && int.TryParse(matches.Groups[2].Value, out var mo))
            {
                try
                {
                    return (new DateTime(y, mo, 1), dbName);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return (default(DateTime), dbName);
                }
            }
            return (default(DateTime), dbName);
        }
    }
}
