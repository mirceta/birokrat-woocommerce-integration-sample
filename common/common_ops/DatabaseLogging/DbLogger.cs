using common_ops.Executors.Sql;
using common_ops.Tasks;
using System;
using System.Threading.Tasks;

namespace common_ops.DatabaseLogging
{
    internal class DbLogger : IDbLogger
    {
        private readonly IDatabaseQueryExecutor _queryExecutor;
        private readonly LogDatabaseSqlHandler _logDbHandler;
        private readonly string _connectionString;

        internal DbLogger(IDatabaseQueryExecutor queryExecutor, LogDatabaseSqlHandler logDbHandler, string connectionString)
        {
            _queryExecutor = queryExecutor;
            _logDbHandler = logDbHandler;
            _connectionString = connectionString;
        }

        internal async Task InitializeAsync()
        {
            var query = _logDbHandler.CreateDatabaseQuery();
            await _queryExecutor.ExecuteNonQueryAsync(_connectionString, query);

            query = _logDbHandler.CreateTableQuery();
            await _queryExecutor.ExecuteNonQueryAsync(_connectionString, query);
        }

        public async Task LogAsync(params string[] data)
        {
            var query = _logDbHandler.GetInsertLogQuery(data);

            try
            {
                var task = new TaskWithTimeout(2000, _queryExecutor.ExecuteNonQueryAsync(_connectionString, query));
                await task.Run();
            }
            catch (TimeoutException tex)
            {
                System.Diagnostics.Debug.WriteLine($"Log write failed: {tex}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Log write failed: {ex}");
            }
        }
    }
}
