using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace common_ops.diagnostics.Checks.Database.Checks
{
    /// <summary>
    /// <para>ONLY in testing environment!</para>
    /// Will synchronize records between biroX-KRATEK, biromaster.entity_company_year, and the actual biroX-YYY
    /// databases where YYY is company year. Additionally it will set
    /// biroX-KRATEK.PoslovnaLeta.ZunanjeRacunovodstvo to false, which mean that this birokrat is cut off from sinhro.
    /// Therefore never use in production!
    /// </summary>
    public class Biro_CompanyYearVerifier
    {
        private readonly string _connectionString;

        private const string DATABASE_YEAR_CODE = "'business-year databases'";
        private const string KRATEK_YEAR_CODE = "'KRATEK'";
        private const string BIROMASTER_YEAR_CODE = "'biromaster'";

        /// <summary>
        /// <inheritdoc cref="Biro_CompanyYearVerifier"/>
        /// </summary>
        public Biro_CompanyYearVerifier(string serverName)
        {
            _connectionString = "Server=" + serverName + ";Integrated Security=True;";
        }

        public List<string> VerifyTaxnum(string taxnum)
        {
            List<string> biromasterYears = GetBiromasterYears(taxnum);
            List<string> kratekOznakas = GetKratekOznakas(taxnum);
            List<string> databaseSubstrings = GetDatabasesYears(taxnum);

            var allValues = new HashSet<string>(biromasterYears);
            allValues.UnionWith(kratekOznakas);
            allValues.UnionWith(databaseSubstrings);

            var result = new List<string>();

            foreach (var value in allValues)
            {
                if (string.IsNullOrEmpty(value))
                    continue;

                var foundIn = new List<string>();
                var notFoundIn = new List<string>();

                if (biromasterYears.Contains(value))
                {
                    foundIn.Add(BIROMASTER_YEAR_CODE);
                }
                else
                {
                    notFoundIn.Add(BIROMASTER_YEAR_CODE);
                }

                if (kratekOznakas.Contains(value))
                {
                    foundIn.Add(KRATEK_YEAR_CODE);
                }
                else
                {
                    notFoundIn.Add(KRATEK_YEAR_CODE);
                }

                if (databaseSubstrings.Contains(value))
                {
                    foundIn.Add(DATABASE_YEAR_CODE);
                }
                else
                {
                    notFoundIn.Add(DATABASE_YEAR_CODE);
                }

                string foundInString = string.Join(" and ", foundIn);
                string notFoundInString = string.Join(" or ", notFoundIn);
                if (!string.IsNullOrEmpty(notFoundInString))
                    result.Add(value + " was found in " + foundInString + " but not in " + notFoundInString);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    USE [biro" + taxnum + @"-KRATEK];
                    SELECT oznaka, oznakaleta FROM [dbo].[SifreOperaterjev]";

                var recordsToUpdate = new List<(string oznaka, string oznakaleta)>();

                bool noErrors = true;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            string id = reader.GetString(0);
                            string oznakaleta = reader.GetString(1);

                            if (!databaseSubstrings.Contains(oznakaleta))
                            {
                                recordsToUpdate.Add((id, oznakaleta));
                            }
                        }
                        catch (Exception ex)
                        {
                            noErrors = false;
                        }
                    }
                }

                // Find the alphabetically highest value in databaseSubstrings
                string highestValue = databaseSubstrings.OrderByDescending(s => s).FirstOrDefault();

                if (highestValue != null)
                {
                    foreach (var (id, oznakaleta) in recordsToUpdate)
                    {
                        var updateCmd = connection.CreateCommand();
                        updateCmd.CommandText = @"
                            USE [biro" + taxnum + @"-KRATEK];
                            UPDATE [dbo].[SifreOperaterjev]
                            SET oznakaleta = @highestValue
                            WHERE oznaka = @id";

                        updateCmd.Parameters.AddWithValue("@highestValue", highestValue);
                        updateCmd.Parameters.AddWithValue("@id", id);

                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
            return result;
        }

        public void TurnOffSinhroInKratekPoslovnaLeta(string taxnum)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                    use [biro" + taxnum + @"-KRATEK]
                    UPDATE PoslovnaLeta 
                    SET ZunanjeRacunovodstvo = 0 
                    WHERE ZunanjeRacunovodstvo = -1";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void SyncCompanyYearFromVerification(string taxnum)
        {
            /*
             implements
             if found in databaseSubstrings, then add wherever its missing
             if not found in databaseSubstrings, then delete wherever its found
             */
            List<string> verificationResults = VerifyTaxnum(taxnum);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var result in verificationResults)
                    {
                        var parts = result.Split(new[] { " was found in ", " but not in " }, StringSplitOptions.None);
                        if (parts.Length != 3) continue;

                        string value = parts[0];
                        string foundIn = parts[1];
                        string notFoundIn = parts[2];

                        if (foundIn.Contains(DATABASE_YEAR_CODE))
                        {
                            if (notFoundIn.Contains(BIROMASTER_YEAR_CODE))
                            {
                                AddToBiromaster(connection, transaction, value, taxnum);
                            }
                            if (notFoundIn.Contains(KRATEK_YEAR_CODE))
                            {
                                string kratekDatabase = "biro" + taxnum + "-KRATEK";
                                string poslovnaLetaTable = "[" + kratekDatabase + "].dbo.PoslovnaLeta";
                                AddToKratekOznakas(connection, transaction, poslovnaLetaTable, value);
                            }
                        }
                        else
                        {
                            if (foundIn.Contains(BIROMASTER_YEAR_CODE))
                            {
                                DeleteFromBiromaster(connection, transaction, taxnum, value);
                            }
                            if (foundIn.Contains(KRATEK_YEAR_CODE))
                            {
                                string kratekDatabase = "biro" + taxnum + "-KRATEK";
                                string poslovnaLetaTable = "[" + kratekDatabase + "].dbo.PoslovnaLeta";
                                DeleteFromKratekOznakas(connection, transaction, poslovnaLetaTable, value);
                            }
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        public List<string> GetBiroKratekMatches()
        {
            var matches = new List<string>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT name 
                    FROM sys.databases 
                    WHERE name LIKE 'biro%-KRATEK'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var dbName = reader["name"].ToString();
                        var start = dbName.IndexOf("biro") + "biro".Length;
                        var end = dbName.IndexOf("-KRATEK");

                        if (start >= 0 && end > start)
                        {
                            var match = dbName.Substring(start, end - start);
                            matches.Add(match);
                        }
                    }
                }
            }

            return matches;
        }

        private List<string> GetDatabasesYears(string taxnum)
        {
            var databases = new List<string>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT name 
                    FROM sys.databases 
                    WHERE name LIKE 'biro" + taxnum + @"%' 
                    AND name NOT LIKE '%KRATEK%' 
                    AND name NOT LIKE '%SINHRO%'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        databases.Add(reader["name"].ToString());
                    }
                }
            }

            return databases
                .Where(name => name.Contains("-"))
                .Select(name => name.Substring(name.IndexOf("-") + 1))
                .ToList();
        }

        private List<string> GetBiromasterYears(string taxnum)
        {
            var years = new List<string>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    USE biromaster;
                    select b.year_code from entity_company a, entity_company_year b
                    where b.fk_entity_company_id = a.pk_entity_company_id
                    and a.tax_number = '" + taxnum + @"'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        years.Add(reader["year_code"].ToString());
                    }
                }
            }

            return years;
        }

        private List<string> GetKratekOznakas(string taxnum)
        {
            var oznakas = new List<string>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    USE [biro" + taxnum + @"-KRATEK];
                    SELECT oznaka FROM PoslovnaLeta";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        oznakas.Add(reader["oznaka"].ToString());
                    }
                }
            }

            return oznakas;
        }

        private void AddToBiromaster(SqlConnection connection, SqlTransaction transaction, string newYearCode, string taxNumber)
        {
            int entityCompanyId = GetEntityCompanyId(connection, transaction, taxNumber);
            string year = "2022";
            string yearcode = newYearCode;
            InsertIntoEntityCompanyYear(entityCompanyId, DateTime.Now,
                DateTime.Now, int.Parse(year), yearcode, 8043, 8043, 0, true);
        }

        private int GetEntityCompanyId(SqlConnection connection, SqlTransaction transaction, string taxNumber)
        {
            var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
                USE biromaster;
                SELECT pk_entity_company_id 
                FROM entity_company 
                WHERE tax_number = @taxNumber";
            cmd.Parameters.AddWithValue("@taxNumber", taxNumber);

            var entityCompanyId = (int?)cmd.ExecuteScalar();
            if (entityCompanyId == null)
            {
                throw new Exception("No entity company found with the specified tax number.");
            }

            return entityCompanyId.Value;
        }

        private void AddToKratekOznakas(SqlConnection connection, SqlTransaction transaction, string table, string value)
        {
            var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
                USE " + table.Split('.')[0] + @";
                INSERT INTO " + table + " (oznaka) VALUES (@value)";
            cmd.Parameters.AddWithValue("@value", value);
            cmd.ExecuteNonQuery();
        }

        private void DeleteFromBiromaster(SqlConnection connection, SqlTransaction transaction, string taxNumber, string yearCode)
        {
            var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
                USE biromaster;
                DELETE FROM entity_company_year 
                WHERE year_code IN (
                    SELECT b.year_code 
                    FROM entity_company a, entity_company_year b 
                    WHERE b.fk_entity_company_id = a.pk_entity_company_id 
                      AND a.tax_number = @taxNumber
                      AND b.year_code = @yearCode
                );";
            cmd.Parameters.AddWithValue("@taxNumber", taxNumber);
            cmd.Parameters.AddWithValue("@yearCode", yearCode);
            cmd.ExecuteNonQuery();
        }

        private void DeleteFromKratekOznakas(SqlConnection connection, SqlTransaction transaction, string table, string value)
        {
            var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
                USE " + table.Split('.')[0] + @";
                DELETE FROM " + table + " WHERE oznaka = @value";
            cmd.Parameters.AddWithValue("@value", value);
            cmd.ExecuteNonQuery();
        }

        public void InsertIntoEntityCompanyYear(
            int fkEntityCompanyId,
            DateTime createdDt,
            DateTime modifiedDt,
            int year,
            string yearCode,
            int localVersion,
            int remoteVersion,
            int remotePartnershipId,
            bool isActive)
        {
            // SQL Insert Statement
            string query = @"
            use [biromaster]
            INSERT INTO [dbo].[entity_company_year]
            ([fk_entity_company_id], [created_dt], [modified_dt], [year], [year_code], [local_version], [remote_version], [remote_partnership_id], [is_active])
            VALUES
            (@FkEntityCompanyId, @CreatedDt, @ModifiedDt, @Year, @YearCode, @LocalVersion, @RemoteVersion, @RemotePartnershipId, @IsActive)";

            // Using statement for automatic disposal of the SqlConnection
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Create SqlCommand
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to SqlCommand
                    command.Parameters.AddWithValue("@FkEntityCompanyId", fkEntityCompanyId);
                    command.Parameters.AddWithValue("@CreatedDt", createdDt);
                    command.Parameters.AddWithValue("@ModifiedDt", modifiedDt);
                    command.Parameters.AddWithValue("@Year", year);
                    command.Parameters.AddWithValue("@YearCode", yearCode);
                    command.Parameters.AddWithValue("@LocalVersion", localVersion);
                    command.Parameters.AddWithValue("@RemoteVersion", remoteVersion);
                    command.Parameters.AddWithValue("@RemotePartnershipId", remotePartnershipId);
                    command.Parameters.AddWithValue("@IsActive", isActive);

                    // Open the connection
                    connection.Open();

                    // Execute the command
                    command.ExecuteNonQuery();

                    // The connection is automatically closed when the using block is exited
                }
            }
        }
    }
}

