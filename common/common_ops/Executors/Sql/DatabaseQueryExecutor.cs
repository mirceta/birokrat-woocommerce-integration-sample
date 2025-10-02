using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace common_ops.Executors.Sql
{
    /// <summary>
    /// Provides methods to execute SQL queries against a database and process the results.
    /// </summary>
    public class DatabaseQueryExecutor : IDatabaseQueryExecutor
    {
        internal readonly WrappedSqlReader _sqlReader = new WrappedSqlReader();

        /// <summary>
        /// Executes a SQL query and returns all rows as a list of strings.
        /// </summary>
        /// <param name="connectionString">The connection string to the database where the query will be executed.</param>
        /// <param name="query">The SQL query to execute.</param>
        /// <returns>A list of strings where each string represents a row from the query results.</returns>
        /// <remarks>
        /// This method opens a SQL connection, executes the specified query using SqlDataReader, and collects the rows. 
        /// Each row is processed into a single string. Attributes on each row are separated by ||. Example: Janez||Novak||3.1.1989.
        /// </remarks>
        public async Task<List<string>> CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(string connectionString, string query)
        {
            List<string> result = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    result = await _sqlReader.ReadAllReaderRows(reader);
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return result;
        }

        /// <summary>
        /// Executes a SQL query and returns the first column of the first row in the result set returned by the query. Use this for
        /// UPDATE, DELETE and INSERT.
        /// </summary>
        /// <param name="connectionString">The connection string to the database where the query will be executed.</param>
        /// <param name="query">The SQL query to execute.</param>
        /// <returns>The value of the first column of the first row as a string, or an empty string if there's an error or no result.</returns>
        /// <remarks>
        /// This method opens a SQL connection, executes a non query, and returns the result. 
        /// If an exception occurs or the result is null, it returns an empty string and logs the error to the console. otherwise it will
        /// return numbers of rows affected. 
        /// </remarks>
        public async Task<string> ExecuteNonQueryAsync(string connectionString, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    var result = await command.ExecuteNonQueryAsync();
                    return result.ToString() ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return string.Empty;
                }
            }
        }
    }
}
