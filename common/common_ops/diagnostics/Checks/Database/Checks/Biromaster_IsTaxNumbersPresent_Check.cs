using common_ops.diagnostics.Checks.Database.Utils;
using common_ops.diagnostics.Constants;
using common_ops.Executors.Sql;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Database.Checks
{
    /// <summary>
    /// Performs if TaxNumber is present in entityCompany in biromaster.
    /// Results are determined based on whether any TaxNumber is retrieved
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: Entity company name</para>
    /// </summary>
    public class Biromaster_IsTaxNumbersPresent_Check : ICheck
    {
        private readonly IDatabaseQueryExecutor _databaseQueryExecutor;
        private readonly string _connectionString;
        private readonly string _taxNumber;

        /// <summary>
        /// <inheritdoc cref="Biromaster_IsTaxNumbersPresent_Check"/>
        /// </summary>
        public Biromaster_IsTaxNumbersPresent_Check(IDatabaseQueryExecutor databaseQueryExecutor, string connectionString, string taxNumber)
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
            var content = await _databaseQueryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, SqlQueries.GetVerifyTaxNumber(_taxNumber));
            return new ResultRecord(content.Any(), GetType().Name, content.ToArray());
        }
    }
}
