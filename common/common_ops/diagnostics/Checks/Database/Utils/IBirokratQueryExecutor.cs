using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Database.Utils
{
    public interface IBirokratQueryExecutor
    {
        Task<List<string>> GetAllDatabases_ThatMatchesTaxNumberAsync(string connectionString, string taxNumber = "");
        Task<List<string>> GetAllTaxNumbers_FromYearcodeDatabasesAsync(string connectionString);
        Task<List<string>> GetCoreDatabasesAsync(string connectionString);
        Task<List<string>> GetYearcodeDatabases_ThatMatchesTaxNumberAsync(string connectionString, string taxNumber = "");
        Task<List<string>> GetYearcodes_ThatMatchesTaxNumberAsync(string connectionString, string taxNumber = "");
    }
}