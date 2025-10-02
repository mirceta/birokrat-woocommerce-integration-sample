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
    /// Performs SifreOperaterjev check for the provided TaxNumber. Will return true if there exists any operater that can login to bironext
    /// and all operaters have valid (not null) yearcodes
    ///
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: operater operater||yearcode pairs, separated with <c>||</c>. We have a row for each operater and their
    /// corresponding yearcode. If this yearcode's database is not present on the sql server, then this yearcode will be postfixed with <c>-ERROR</c>
    /// </para>
    /// <remarks>
    /// <para>Example of <see cref="ResultRecord.AdditionalInfo"/>:</para>
    /// <list type="bullet">
    /// <item><description>databases in SQL server</description></item>
    /// <item><description>biro00000000-I13</description></item>
    /// <item><description>biro00000000-J13</description></item>
    /// <item> <description>operater,yearcode:</description></item>
    /// <item><description>raco,I13</description></item>
    /// <item><description>kris,H18</description></item>
    /// <item><description><see cref="ResultRecord.AdditionalInfo"/></description></item>
    /// <item><description>[raco||I13],[kris||H18-ERROR]</description></item>
    /// </list>
    /// </remarks>
    /// </summary>
    public class BiroNext_SifreOperaterjev_Check : ICheck
    {
        private readonly IBirokratQueryExecutor _birokratQueryExecutor;
        private readonly IDatabaseQueryExecutor _databaseQueryExecutor;
        private readonly string _connectionString;
        private readonly string _taxNumber;

        /// <summary>
        /// <inheritdoc cref="BiroNext_SifreOperaterjev_Check"/>
        /// </summary>
        public BiroNext_SifreOperaterjev_Check(IBirokratQueryExecutor birokratQueryExecutor, IDatabaseQueryExecutor databaseQueryExecutor, string connectionString, string taxNumber)
        {
            _birokratQueryExecutor = birokratQueryExecutor;
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
            var content = await _databaseQueryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, SqlQueries.GetSifreOperaterjev(_taxNumber));
            var checkResult = false;
            var operatersAndYearcodes = new List<(string operater, string yearcode)>();
            var finalOperatersAndYearcodes = new List<(string operater, string yearcode)>();

            int operater = 0;
            int yearcode = 1;

            if (content.Any())
            {
                foreach (var item in content)
                {
                    var row = item.Split(new[] { TextConstants.DELIMITER }, StringSplitOptions.None);

                    if (row.Any(x => x.Equals(TextConstants.NULL_FIELD, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        operatersAndYearcodes.Add((row[operater], row[yearcode]));
                        continue;
                    }
                    operatersAndYearcodes.Add((row[operater], row[yearcode]));
                }
                operatersAndYearcodes = operatersAndYearcodes.Distinct().ToList();
                //if (operatersAndYearcodes.Any(x => x.yearcode.Trim().Equals(TextConstants.NULL_FIELD, StringComparison.CurrentCultureIgnoreCase)))
                //    checkResult = false;

                var databases = await _birokratQueryExecutor.GetAllDatabases_ThatMatchesTaxNumberAsync(
                    _connectionString,
                    _taxNumber);

                for (int i = 0; i < operatersAndYearcodes.Count; i++)
                {
                    if (!databases.Any(x => x.Contains(operatersAndYearcodes[i].yearcode)))
                    {
                        finalOperatersAndYearcodes.Add((operatersAndYearcodes[i].operater, operatersAndYearcodes[i].yearcode + " " + TextConstants.POSTFIX_ERROR));
                    }
                    else
                    {
                        finalOperatersAndYearcodes.Add(operatersAndYearcodes[i]);
                        checkResult = true;
                    }
                }
            }

            return new ResultRecord(checkResult, GetType().Name, BuildAditionalInfo(finalOperatersAndYearcodes));
        }

        private string[] BuildAditionalInfo(List<(string operater, string yearcode)> content)
        {
            return content.Select(x => $"{x.operater}{TextConstants.DELIMITER}{x.yearcode}").ToArray();
        }
    }
}
