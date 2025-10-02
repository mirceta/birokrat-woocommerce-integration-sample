using common_ops.diagnostics.Checks.Database.Utils;
using common_ops.diagnostics.Constants;
using common_ops.Executors.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Database.Checks
{
    /// <summary>
    /// Performs a collation check for the server and yearcode databases. 
    /// Results are determined based on whether the collation matches the specified default collation provided in constructor as argument.
    /// 
    /// <para>If the default collation is not met, results will return as database||collation, separated with <c>||</c>.
    /// The overall result will also be <c>true</c> if a yearcode database's collation is null but the main server's collation matches the `_defaultCollation`.</para>
    ///
    /// <remarks>
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains:</para>
    /// <list type="bullet">
    /// <item>
    /// <description>[0]: Main SQL Server collation.</description>
    /// </item>
    /// <item>
    /// <description>[1 and subsequent]: Distinct yearcode database collations.</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// If atitional info only contains one record with default collation all databases share the same collation
    /// </summary>
    public class SqlServer_Collation_Check : ICheck
    {
        private readonly IBirokratQueryExecutor _birokratQueryExecutor;
        private readonly IDatabaseQueryExecutor _databaseQueryExecutor;
        private readonly string _connectionString;
        private readonly string _taxNumber;
        private readonly string _defaultCollation;

        /// <summary>
        /// <inheritdoc cref="SqlServer_Collation_Check"/>
        /// </summary>
        public SqlServer_Collation_Check(IBirokratQueryExecutor birokratQueryExecutor, IDatabaseQueryExecutor databaseQueryExecutor, string connectionString, string taxNumber, string defaultCollation = "Slovenian")
        {
            _birokratQueryExecutor = birokratQueryExecutor;
            _databaseQueryExecutor = databaseQueryExecutor;
            _connectionString = connectionString;
            _taxNumber = taxNumber;
            _defaultCollation = defaultCollation;
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                return await Work();
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
        }

        private async Task<ResultRecord> Work()
        {
            var content = new List<(string database, string collation)>();

            string mainServerCollation = await GetMainServerColation(_connectionString);
            content.Add(("Server", mainServerCollation));

            var yearcodeDbCollations = await GetYearcodeDatabaseCollations(_connectionString, _taxNumber, _defaultCollation);
            content.AddRange(yearcodeDbCollations);

            var result = DetermineOperationResult(content, _defaultCollation);

            return new ResultRecord(result, GetType().Name, BuildResultRecordAditionalInfo(content));
        }

        private string[] BuildResultRecordAditionalInfo(List<(string database, string collation)> content)
        {
            return content
                .Distinct()
                .Select(x => string.IsNullOrEmpty(x.database) ? $"{x.collation}" : $"{x.database}{TextConstants.DELIMITER}{x.collation}")
                .ToArray();
        }

        private bool DetermineOperationResult(List<(string database, string collation)> content, string defaultCollation)
        {
            var result = true;
            if (content.Count == 0)
                return false;

            if (!content[0].collation.StartsWith(defaultCollation, StringComparison.CurrentCultureIgnoreCase))
                return false;

            for (int i = 1; i < content.Count; i++)
            {
                if (!content[i].collation.StartsWith(defaultCollation, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!content[i].collation.Equals(TextConstants.NULL_FIELD, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return result;
        }

        private async Task<string> GetMainServerColation(string connectionString)
        {
            var mainServerCollation = await _databaseQueryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(connectionString, SqlQueries.GetDefaultServerColation());
            return mainServerCollation.FirstOrDefault();
        }

        private async Task<List<(string database, string collation)>> GetYearcodeDatabaseCollations(string connectionString, string taxNumber, string defaultCollation)
        {
            var yearcodeDatabases = await _birokratQueryExecutor.GetAllDatabases_ThatMatchesTaxNumberAsync(
                connectionString,
                taxNumber);

            var results = new List<(string collation, string database)>();
            foreach (var db in yearcodeDatabases)
            {
                var result = await _databaseQueryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(connectionString, SqlQueries.GetDatabaseCollation(db));
                var collation = result.FirstOrDefault();

                if (collation.StartsWith(defaultCollation, StringComparison.CurrentCultureIgnoreCase))
                    results.Add(("", collation));
                else
                    results.Add((db, collation));
            }
            return results.Distinct().ToList();
        }
    }
}
