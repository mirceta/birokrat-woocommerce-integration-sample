using System.Threading.Tasks;

namespace common_ops.Executors.Sql
{
    public interface ISqlUtils
    {
        Task<bool> CheckSqlServer(string connectionString);
        string GenerateConnectionString(string serverName);
        string ParseSqlServerToRealName(string serverName);
    }
}