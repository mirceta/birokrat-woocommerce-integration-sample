using common_ops.diagnostics.Constants;
using common_ops.Executors.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Database.Checks
{
    /// <summary> TODO !!!!!!!
    /// Checks for core databases on server (application, biromaster and configuration)
    /// Results are determined based on whether any databases are found. Will return false if any of core databases is not present
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: databaseName or in case of an error databaseName||null. Separated with <c>||</c> </para>
    /// </summary>
    public class BiroSinhro_Bazure1vsBazure5_Check : ICheck
    {
        private readonly IDatabaseQueryExecutor _databaseQueryExecutor;

        /// <summary>
        /// <inheritdoc cref="BiroSinhro_Bazure1vsBazure5_Check"/>
        /// </summary>
        public BiroSinhro_Bazure1vsBazure5_Check(IDatabaseQueryExecutor databaseQueryExecutor)
        {
            _databaseQueryExecutor = databaseQueryExecutor;
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                return await Work();
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "ERROR: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
        }

        private async Task<ResultRecord> Work()
        {
            var connectionString = "Server=192.168.0.45;User Id=StatsBazure;Password=20Bazts25Staure;";
            var query = "exec [ProductionSync].[dbo].[StatsBazure1AndBazure5]";
            var additionalInfo = new List<string>();

            var results = await _databaseQueryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(connectionString, query);

            if (!results.Any())
                return new ResultRecord(false, GetType().Name, "No records retrieved" + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR);

            string[] separators = new string[] { TextConstants.DELIMITER };
            foreach (var row in results)
            {
                var check = CheckFields(row, separators);
                if (!string.IsNullOrEmpty(check))
                    additionalInfo.Add(check);
            }

            return new ResultRecord(true, GetType().Name, additionalInfo.ToArray());
        }

        private string CheckFields(string row, string[] separator)
        {
            bool checkResult = false;

            string[] fields = row.Split(separator, StringSplitOptions.None);
            var info = new StringBuilder().AppendLine(fields[1]);

            int index = 5;
            if (!fields[index].Equals(fields[index + 3], StringComparison.OrdinalIgnoreCase))
            {
                info.AppendLine($"Program Version Mismatch! Bazure 1: {fields[index]}, Bazure 5: {fields[index + 3]}");
                checkResult = true;
            }
            index++;
            if (!fields[index].Equals(fields[index + 3], StringComparison.OrdinalIgnoreCase))
            {
                info.AppendLine($"YearCode Mismatch! Bazure 1: {fields[index]}, Bazure 5: {fields[index + 3]}");
                checkResult = true;
            }
            index++;
            if (!fields[index].Equals(fields[index + 3], StringComparison.OrdinalIgnoreCase))
            {
                info.AppendLine($"Files Waiting Mismatch! Bazure 1: {fields[index]}, Bazure 5: {fields[index + 3]}");
                checkResult = true;
            }

            if (checkResult)
                return info.ToString();

            return string.Empty;
        }
    }
}
