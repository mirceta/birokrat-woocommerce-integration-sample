using Microsoft.Data.SqlClient;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.SystemConsole.Themes;
using System;

namespace logging
{
    public class BironextWoocommerceHubLogging
    {

        static string logging_sql_connection_string = "Server=[SERVER];Database=LogDb;Trusted_Connection=True;";
        //localhost\\MSSQLSERVER2
        public static void LoggingSetup(string logging_sql_server) {

            logging_sql_connection_string = logging_sql_connection_string.Replace("[SERVER]", logging_sql_server);
            Log.Logger = new LoggerConfiguration()
                            .WriteTo
                            .MSSqlServer(
                                connectionString: logging_sql_connection_string,
                                sinkOptions: new MSSqlServerSinkOptions {
                                    TableName = "BiroWooLogEvents",
                                    SchemaName = "dbo",
                                    AutoCreateSqlTable = true
                                }
                            )
                            .WriteTo
                            .Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                                theme: AnsiConsoleTheme.Literate)
                            .CreateLogger();
            Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));
            Log.Information("Started program");
            //Log.CloseAndFlush();
            CheckLogDatabaseExists();

        }

        private static bool CheckLogDatabaseExists() {

            // programmer must insure that LogDb database exists

            string sqlCreateDBQuery;
            bool result = false;

            try {
                var tmpConn = new SqlConnection(logging_sql_connection_string);

                sqlCreateDBQuery = "SELECT database_id FROM sys.databases WHERE Name = 'LogDb'";

                using (tmpConn) {
                    using (SqlCommand sqlCmd = new SqlCommand(sqlCreateDBQuery, tmpConn)) {
                        tmpConn.Open();

                        object resultObj = sqlCmd.ExecuteScalar();

                        int databaseID = 0;

                        if (resultObj != null) {
                            int.TryParse(resultObj.ToString(), out databaseID);
                        }

                        tmpConn.Close();

                        result = (databaseID > 0);
                    }
                }
            } catch (Exception ex) {
                throw ex;
            }

            return result;
        }
    }
}
