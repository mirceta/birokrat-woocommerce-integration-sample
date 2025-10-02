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
    /// Performs SifreOperaterjev check for the provided TaxNumber. TaxNumber is ment to be 00000000 which is used by pinger. Will return true if there exists
    /// any operater that can login to bironext and operater have valid (not null) yearcode. 
    ///
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: operater operater||yearcode pairs, separated with <c>||</c>. If this yearcode's database
    /// is not present on the sql server, then this yearcode will be postfixed with <c>ERROR</c>. If repair option is chosen it will update the yearcode to latest
    /// one and will return repair result in 2nd row as 1||yearcode REPAIR
    /// </para>
    /// </summary>

    public class BiroNext_PingerSifraOperaterjaAndPoslovnoLeto_CheckAndRepair : ICheck
    {
        private readonly IBirokratQueryExecutor _birokratQueryExecutor;
        private readonly IDatabaseQueryExecutor _databaseQueryExecutor;
        private readonly string _connectionString;
        private readonly string _taxNumber;

        /// <summary>
        /// <inheritdoc cref="BiroNext_PingerSifraOperaterjaAndPoslovnoLeto_CheckAndRepair"/>
        /// </summary>
        public BiroNext_PingerSifraOperaterjaAndPoslovnoLeto_CheckAndRepair(IBirokratQueryExecutor birokratQueryExecutor, IDatabaseQueryExecutor databaseQueryExecutor, string connectionString, string taxNumber = "00000000")
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
            var content = await _databaseQueryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, SqlQueries.GetSifraOperaterjaAndYearcodeForPinger());
            var checkResult = true;

            var aditionalInfo = content;
            try
            {
                var operatersAndYearcodes = GenerateData(content);
                string postfix = TextConstants.POSTFIX_OK;
                string operater = operatersAndYearcodes.operater;
                string yearcode = operatersAndYearcodes.yearcode;

                var yearcodes = await _birokratQueryExecutor.GetYearcodes_ThatMatchesTaxNumberAsync(
                  _connectionString,
                  _taxNumber);

                if (!yearcodes.Any(x => x.Contains(operatersAndYearcodes.yearcode)))
                {
                    aditionalInfo[0] = aditionalInfo[0] + " " + TextConstants.POSTFIX_WARNING;
                    yearcode = yearcodes.First();
                    postfix = TextConstants.POSTFIX_REPAIR;

                    var result = await _databaseQueryExecutor.ExecuteNonQueryAsync(
                        _connectionString,
                        SqlQueries.UpdateSifraOperaterjaForPinger(yearcode));

                    if (!result.Contains("1"))
                        throw new Exception();

                    aditionalInfo.Add(GenerateAditionalInfo(operater, yearcode, postfix));
                }

                return new ResultRecord(checkResult, GetType().Name, aditionalInfo.ToArray());
            }
            catch
            {
                checkResult = false;
            }

            return new ResultRecord(checkResult, GetType().Name, aditionalInfo.FirstOrDefault() + " " + TextConstants.POSTFIX_ERROR);
        }

        private (string operater, string yearcode) GenerateData(List<string> content)
        {
            if (content.Any())
            {
                var row = content.First().Split(new[] { TextConstants.DELIMITER }, StringSplitOptions.None);

                if (!row.Any(x => x.Equals(TextConstants.NULL_FIELD, StringComparison.CurrentCultureIgnoreCase)))
                    return (row[0], row[1]);
            }
            throw new Exception();
        }

        private string GenerateAditionalInfo(string operater, string yearcode, string postfix)
        {
            return operater + TextConstants.DELIMITER + yearcode + " " + postfix;
        }
    }
}
