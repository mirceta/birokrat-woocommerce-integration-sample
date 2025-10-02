using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops.Executors.Sql
{
    public interface IDatabaseQueryExecutor
    {
        Task<List<string>> CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(string connectionString, string query);
        Task<string> ExecuteNonQueryAsync(string connectionString, string query);
    }
}