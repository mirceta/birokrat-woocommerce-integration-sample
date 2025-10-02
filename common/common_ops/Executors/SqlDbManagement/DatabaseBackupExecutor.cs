using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace common_ops.Executors.SqlDbManagement
{
    public class DatabaseBackupExecutor
    {
        #region BACKUP
        public async Task<string> BackupDatabase(DatabaseInfo info)
        {
            string sql = GetBackupDatabaseQuery(info.ServerBackupFolder, info.DatabaseName);
            try
            {
                using (SqlConnection connection = new SqlConnection(info.ConnectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                        return $"Backup of database '{info.DatabaseName}' completed successfully.";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public string GetDefaultLocation_BackupPath(string connectionString)
        {
            string defaultBackupPath = string.Empty;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand(GetDefaultBackupPathQuery(), connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                defaultBackupPath = reader.GetString(1); // Get the backup directory from the second column
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { };

            return defaultBackupPath;
        }

        #endregion

        #region DROP DATABSE
        /// <summary>
        /// Will fail if no database with set name is available. But is in try block so just continue
        /// </summary>
        public async Task<string> DropDatabaseAsync(DatabaseInfo info)
        {
            List<string> filePaths = new List<string>();
            try
            {
                using (var connection = new SqlConnection(info.ConnectionString))
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    var checkDbExistsCommandText = GetIfDatabaseExistQuery(info.DatabaseName);

                    using (var command = new SqlCommand(checkDbExistsCommandText, connection))
                    {
                        int dbExists = (int)await command.ExecuteScalarAsync().ConfigureAwait(false);
                        if (dbExists == 0)
                        {
                            return $"Database '{info.DatabaseName}' does not exist.";
                        }
                    }
                    // Set database to SINGLE_USER to disconnect other users
                    var setSingleUserCommandText = $@"
                    ALTER DATABASE [{info.DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    ";
                    using (var command = new SqlCommand(setSingleUserCommandText, connection))
                    {
                        //this can throw exeption is no database is server. It will just skipp drop step and will restore databases as intended
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }


                    // Query to get the physical file names of the database files
                    var getFilePathsCommandText = $@"
                    SELECT physical_name FROM sys.master_files
                    WHERE database_id = DB_ID('{info.DatabaseName}');";

                    using (var command = new SqlCommand(getFilePathsCommandText, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                filePaths.Add(reader.GetString(0));
                            }
                        }
                    }

                    var dropDatabaseCommandText = $@"
                    DROP DATABASE IF EXISTS [{info.DatabaseName}];
                    ";
                    using (var command = new SqlCommand(dropDatabaseCommandText, connection))
                    {
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }

                // Assuming file removal is required and the app has permissions - this is just a safetycheck
                foreach (var filePath in filePaths)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }

                return $"Database '{info.DatabaseName}' dropped succesfully";
            }
            catch (Exception ex)
            {
                return $"Error dropping database: {ex.Message}";
            }
        }

        private string GetIfDatabaseExistQuery(string databaseName)
        {
            return $@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}')
                BEGIN
                    SELECT 0
                END
                ELSE
                BEGIN
                    SELECT 1
                END";
        }
        #endregion

        #region RESTORE DATABASE
        /// <summary>
        /// Restores a SQL Server database from a specified backup file, using the MOVE clause to relocate the database files.
        /// </summary>
        /// <remarks>
        /// The method constructs and executes a RESTORE DATABASE command that:
        /// - Specifies the source backup file (.bak) from which to restore the database.
        /// - Utilizes the REPLACE and RECOVERY options to ensure the database is fully operational post-restore.
        /// - Employs the MOVE clause to specify new locations for the database's data (.mdf) and log (.ldf) files, accommodating scenarios where the
        /// original file paths within the backup are not suitable or available in the current environment.
        /// This approach enables the restoration of databases in environments different from where the backup was originally taken, such as different
        /// server setups or directory structures.
        ///</remarks>
        public async Task<string> RestoreDatabaseAsync(DatabaseInfo info)
        {
            try
            {
                using (var connection = new SqlConnection(info.ConnectionString))
                {
                    await connection.OpenAsync().ConfigureAwait(false);

                    // RestoreDbForm database from backup
                    var path = Path.Combine(info.ServerBackupFolder, info.DatabaseName);
                    var target = Path.Combine(info.DataFolder, info.DatabaseName);
                    var restoreDatabaseCommandText = $@"
                    RESTORE DATABASE [{info.DatabaseName}] FROM DISK = N'{path}.bak' WITH REPLACE, RECOVERY,
                    MOVE '{info.DatabaseName}' TO '{target}.mdf',
                    MOVE '{info.DatabaseName}_log' TO '{target}.ldf';
                    ALTER DATABASE [{info.DatabaseName}] SET MULTI_USER;
                    ";

                    using (var command = new SqlCommand(restoreDatabaseCommandText, connection))
                    {
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                    return $"Database '{info.DatabaseName}' restored succesfully";
                }
            }
            catch (Exception ex)
            {
                return $"Error restoring database '{info.DatabaseName}': {ex.Message}";
            }
        }

        public async Task<string> RestoreDatabaseLogicalAsync(DatabaseInfo info)
        {
            try
            {
                using (var connection = new SqlConnection(info.ConnectionString))
                {
                    await connection.OpenAsync().ConfigureAwait(false);

                    // Step 1: Retrieve logical file names from the backup
                    var fileListOnlyCommandText = $"RESTORE FILELISTONLY FROM DISK = N'{Path.Combine(info.ServerBackupFolder, info.DatabaseName)}.bak'";
                    var logicalFileNames = new Dictionary<string, string>();

                    using (var command = new SqlCommand(fileListOnlyCommandText, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                var logicalName = reader["LogicalName"].ToString();
                                var type = reader["Type"].ToString();
                                logicalFileNames[type] = logicalName;
                            }
                        }
                    }

                    // Step 2: Prepare restore command with the retrieved logical names
                    var dataFileLogicalName = logicalFileNames["D"]; // Data file logical name
                    var logFileLogicalName = logicalFileNames["L"];  // Log file logical name

                    var targetDataFile = Path.Combine(info.DataFolder, $"{info.DatabaseName}.mdf");
                    var targetLogFile = Path.Combine(info.DataFolder, $"{info.DatabaseName}_log.ldf");

                    var restoreDatabaseCommandText = $@"
                    RESTORE DATABASE [{info.DatabaseName}] FROM DISK = N'{Path.Combine(info.ServerBackupFolder, info.DatabaseName)}.bak' WITH REPLACE, RECOVERY,
                    MOVE '{dataFileLogicalName}' TO '{targetDataFile}',
                    MOVE '{logFileLogicalName}' TO '{targetLogFile}';
                    ALTER DATABASE [{info.DatabaseName}] SET MULTI_USER;
                    ";

                    // Step 3: Execute the restore command
                    using (var command = new SqlCommand(restoreDatabaseCommandText, connection))
                    {
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    return $"Database '{info.DatabaseName}' restored successfully";
                }
            }
            catch (Exception ex)
            {
                return $"Error restoring database '{info.DatabaseName}': {ex.Message}";
            }
        }
        #endregion

        #region AUXILIARY
        public string GetDefaultLocation_DataPath(string connectionString)
        {
            var path = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(GetDefaultDatabaseLocationQuery(), connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string filePath = reader["FilePath"].ToString();
                        string dbName = Path.GetFileName(filePath);
                        path = filePath.Replace(dbName, string.Empty);
                        break;
                    }
                }
            }
            return path;
        }

        public async Task<IEnumerable<string>> FetchAllDbNamesAsync(string connectionString)
        {
            string query = "SELECT name FROM sys.databases;";
            List<string> databases = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        databases.Add(reader["name"].ToString());
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            await Task.Delay(1000);
            return databases;
        }
        #endregion

        #region SERVICE ACCOUNT AND DATA
        public string GetSqlServerServiceAccount(string connectionString)
        {
            string query = GetServiceAccountQuerry();
            var serviceAccount = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        serviceAccount = reader["service_account"].ToString();
                    }
                }
            }
            return serviceAccount;
        }
        #endregion

        #region Queries
        private string GetBackupDatabaseQuery(string defaultBackupPath, string databaseName)
        {
            return $@"
            BACKUP DATABASE [{databaseName}] 
            TO DISK = N'{defaultBackupPath}\{databaseName}.bak' 
                WITH COPY_ONLY, 
                FORMAT, 
                INIT, 
                NAME = N'backup-{databaseName}', 
                SKIP, 
                NOREWIND, 
                NOUNLOAD, 
                STATS = 10, 
                DESCRIPTION = N'A full backup of [{databaseName}] database';";
        }

        private string GetDefaultBackupPathQuery()
        {
            return @"EXEC master.dbo.xp_instance_regread 
                N'HKEY_LOCAL_MACHINE', 
                N'Software\Microsoft\MSSQLServer\MSSQLServer', 
                N'BackupDirectory';";
        }

        private string GetDefaultDatabaseLocationQuery()
        {
            return @"
            SELECT 
                db.name AS DatabaseName,
                mf.physical_name AS FilePath
            FROM 
                sys.master_files mf
            INNER JOIN 
                sys.databases db ON mf.database_id = db.database_id
            WHERE 
                type_desc = 'ROWS'";
        }

        private string GetServiceAccountQuerry()
        {
            return @"
            SELECT servicename, service_account
            FROM sys.dm_server_services
            WHERE servicename LIKE 'SQL Server (%';";
        }
        #endregion
    }
}
