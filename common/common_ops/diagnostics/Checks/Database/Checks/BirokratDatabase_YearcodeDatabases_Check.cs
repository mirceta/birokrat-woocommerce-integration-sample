using common_ops.diagnostics.Checks.Database.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Database.Checks
{
    /// <summary>
    /// Checks for yearcode databases on server. Will also check if -KRATEK and -SINHRO are present. Will return
    /// false if -KRATEK and -SINHRO or no databases with TaxNumber are present.
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: databaseName. In case if -KRATEK or -SINHRO are not found: databaseName||null. Separated with <c>||</c>.
    /// Arry will be empty if no databases for corresponding TaxNumber are found</para>
    /// </summary>
    public class BirokratDatabase_YearcodeDatabases_Check : ICheck
    {
        private readonly IBirokratQueryExecutor _queryExecutor;
        private readonly string _connectionString;
        private readonly string _taxNumber;

        /// <summary>
        /// <inheritdoc cref="BirokratDatabase_YearcodeDatabases_Check"/>
        /// </summary>
        public BirokratDatabase_YearcodeDatabases_Check(IBirokratQueryExecutor queryExecutor, string connectionString, string taxNumber)
        {
            _queryExecutor = queryExecutor;
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
            var davcnaDbs = await _queryExecutor.GetAllDatabases_ThatMatchesTaxNumberAsync(
                _connectionString,
                _taxNumber);

            var check = true;
            List<string> output = new List<string>();

            if (davcnaDbs.Count == 0)
                return new ResultRecord(false, GetType().Name, davcnaDbs.ToArray());

            var regex = new Regex(@"-(kratek|sinhro)$", RegexOptions.IgnoreCase);
            int count = davcnaDbs.Count(x => !regex.IsMatch(x));

            if (count == 0)
            {
                davcnaDbs.Add($"No yearcode database! {TextConstants.POSTFIX_ERROR}");
                return new ResultRecord(false, GetType().Name, davcnaDbs.ToArray());
            }

            foreach (var db in davcnaDbs)
            {
                output.Add($"{db}");
            }

            if (!davcnaDbs.Any(x => x.Contains("-KRATEK")))
            {
                output.Add($"{_taxNumber}-KRATEK{TextConstants.DELIMITER}{TextConstants.NULL_FIELD}");
                check = false;
            }

            if (!davcnaDbs.Any(x => x.Contains("-SINHRO")))
            {
                output.Add($"{_taxNumber}-SINHRO{TextConstants.DELIMITER}{TextConstants.NULL_FIELD}");
                check = false;
            }

            return new ResultRecord(check, GetType().Name, output.ToArray());
        }
    }
}
