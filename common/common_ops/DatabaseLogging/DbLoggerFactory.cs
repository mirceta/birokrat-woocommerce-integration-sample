using common_ops.Executors.Sql;
using System;
using System.Threading.Tasks;

namespace common_ops.DatabaseLogging
{
    /// <summary>
    /// 1. Factory for creating a ready-to-use database logger. Ensures the target logging database and table exist (creating them if needed),
    /// optionally performs housekeeping (e.g., removing old date-suffixed databases), and returns an initialized <see cref="IDbLogger"/> that can be used immediately. 
    /// DatabaseName provided in parameters will be used as a base line database name date-suffixed with year and month. Example BiroNextLogs_2025_12. 
    /// </summary>
    /// <remarks>
    /// 
    /// <para> 2. The <c>Name</c> property specifies the column name in SQL Server,
    /// while the <c>Type</c> property specifies the SQL type
    /// (e.g., INT, NVARCHAR(50), DATETIME, FLOAT). Example of creating a log table schema:
    /// </para>
    /// <code>
    /// var attributes = new[]
    /// {
    ///     new SAttribute { Name = "DateTime", Type = "DATETIME" },
    ///     new SAttribute { Name = "Label", Type = "NVARCHAR(50)" },
    ///     new SAttribute { Name = "Decimal", Type = "FLOAT" },
    ///     new SAttribute { Name = "Whole", Type = "INT" }
    /// };
    /// 
    /// var logger = await new DbLoggerFactory().Build("CommonDbLog",connectionString, attributes);
    /// 
    /// // Insert a log entry
    /// await logger.LogAsync(DateTime.Now.ToString(), "ExampleLabel", "8.9", "1");
    /// </code>
    /// 
    /// <para>3. Database creation guideline:</para>
    /// Always use English(US) number format. Decimal separator must be '.' (dot, never ',').
    /// Do not use culture-specific DateTime formatting. Always insert dates using DateTime.Now.ToString().
    /// </remarks>
    public class DbLoggerFactory
    {
        /// <summary>
        /// <inheritdoc cref="DbLoggerFactory"/>
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="DbLoggerFactory"/>
        /// </remarks>
        public DbLoggerFactory() { }

        public async Task<IDbLogger> Build(string sqlServerName, string databaseName, params SAttribute[] attributes)
        {
            var sqlu = new SqlUtils();
            var connectionString = sqlu.GenerateConnectionString(sqlu.ParseSqlServerToRealName(sqlServerName));

            var test = await sqlu.CheckSqlServer(connectionString);

            if (!test)
                throw new Exception($"Could not connect to sqewl server! Connection string: {connectionString}");

            var queryExecutor = new DatabaseQueryExecutor();

            var dates = new DatabaseCleanup(queryExecutor, connectionString, new DatabaseName(databaseName));
            await dates.RemoveOldDatabases();

            var logger = new DbLogger(
                queryExecutor,
                new LogDatabaseSqlHandler(new DatabaseName(databaseName), attributes),
                connectionString);

            await logger.InitializeAsync();
            return logger;
        }
    }
}
