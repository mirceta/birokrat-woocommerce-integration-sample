using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace common_ops.Executors.Sql
{
    public class SqlUtils : ISqlUtils
    {
        public async Task<bool> CheckSqlServer(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    return true;
                }
                catch (Exception ex)
                {

                }
            }
            return false;
        }

        public bool TryCheckSqlServer(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (Exception ex)
                {

                }
            }
            return false;
        }

        public string ParseSqlServerToRealName(string serverName)
        {
            var name = serverName.Trim().ToLower();

            if (name.Equals("localhost"))
                name = Environment.MachineName;
            else if (name.Contains("localhost\\"))
                name = name.Replace("localhost\\", $"{Environment.MachineName}\\");
            if (name.Contains(".\\"))
                name = name.Replace(".\\", $"{Environment.MachineName}\\");
            else if (name.Trim().Equals("."))
                name = Environment.MachineName;

            return name.ToUpper();
        }

        public string GenerateConnectionString(string serverName)
        {
            serverName = ParseSqlServerToRealName(serverName);

            return $@"Server={serverName};Trusted_Connection=True;Integrated Security=True;Encrypt=False;";
        }
    }
}
