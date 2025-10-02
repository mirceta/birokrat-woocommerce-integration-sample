using common_ops.diagnostics.Checks.Database.Utils;
using common_ops.diagnostics.Constants;
using common_ops.Executors.Sql;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Database.Checks
{
    /// <summary>
    /// Performs APIKey check for the provided TaxNumber. Will return all api keys and corresponding users
    /// Results are determined based on whether any _apiKey is retrieved
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: APIKeys with corresponding user in format: _apiKey||User. Separated with <c>||</c> </para>
    /// </summary>
    public class BiroNext_ApiKeys_Check : ICheck
    {
        private readonly IDatabaseQueryExecutor _databaseQueryExecutor;
        private readonly string _connectionString;
        private readonly string _taxNumber;

        /// <summary>
        /// <inheritdoc cref="BiroNext_ApiKeys_Check"/>
        /// </summary>
        public BiroNext_ApiKeys_Check(IDatabaseQueryExecutor databaseQueryExecutor, string connectionString, string taxNumber)
        {
            _databaseQueryExecutor = databaseQueryExecutor;
            _connectionString = connectionString;
            _taxNumber = taxNumber;
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
            var content = await _databaseQueryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, SqlQueries.GetApiKeys(_taxNumber));
            return new ResultRecord(content.Any(), GetType().Name, content.ToArray());
        }
    }
}
