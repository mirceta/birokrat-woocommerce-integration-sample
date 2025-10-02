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
    /// Performs ProgramVersion check for the provided SqlServer. Results are determined based on whether ProgramVersion is retrieved from database.
    /// Will return false if no version is retrieved or if major version is not matching local Birokrat.exe version.
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: [0] = DatabaseVersion, [1] = BirokratExeVersion, [2] = Major Version check, [3] = Minor Version check</para>
    /// </summary>
    public class BirokratDatabase_ProgramVersion_Check : ICheck
    {
        private readonly IDatabaseQueryExecutor _databaseQueryExecutor;
        private readonly string _connectionString;
        private readonly string _taxNumber;
        private readonly string _birokratExeVersion;
        private readonly bool _isSinhroAndBazure;

        /// <summary>
        /// <inheritdoc cref="BirokratDatabase_ProgramVersion_Check"/>
        /// </summary>
        public BirokratDatabase_ProgramVersion_Check(IDatabaseQueryExecutor databaseQueryExecutor, string connectionString, string taxNumber, string birokratExeVersion, bool isSinhroAndBazure = false)
        {
            _databaseQueryExecutor = databaseQueryExecutor;
            _connectionString = connectionString;
            _taxNumber = taxNumber;
            _birokratExeVersion = birokratExeVersion;
            _isSinhroAndBazure = isSinhroAndBazure;
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
            var info = new List<string>();
            var checkResult = true;

            if (_isSinhroAndBazure)
            {
                var verb = await DetermineVersionFromBiromaster(checkResult, info);
                return verb;
            }

            var verk = await DetermineVersionFromKratek(checkResult, info);
            return verk;

        }

        private async Task<ResultRecord> DetermineVersionFromKratek(bool checkResult, List<string> info)
        {
            var content = await _databaseQueryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(
                                _connectionString,
                                SqlQueries.GetProgramVersionFromKratek(_taxNumber));

            if (content.Any())
            {
                return CompareVersionsAndReturnResult(checkResult, info, content);
            }
            else
            {
                checkResult = false;
            }

            content.Add($"Waiting for bojan to enable nadgradnja over arguments " + TextConstants.POSTFIX_WARNING);
            return new ResultRecord(true, GetType().Name, content.ToArray());
        }

        private async Task<ResultRecord> DetermineVersionFromBiromaster(bool checkResult, List<string> info)
        {
            var content = await _databaseQueryExecutor.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(
                    _connectionString,
                    SqlQueries.GetProgramVersionFomBiromaster(_taxNumber));

            if (content.Count > 0)
            {
                var latestYearData = content.First().Split(new string[] { "||" }, StringSplitOptions.None);
                var latestYear = latestYearData.First();

                var count = content.Where(x => x.StartsWith(latestYear)).ToList().Count;

                if (count > 1)
                    info.Add("Duplicated year in database! This should NOT happen !! " + TextConstants.POSTFIX_ERROR);

                content = new List<string> { latestYearData.Last() };
                return CompareVersionsAndReturnResult(checkResult, info, content);
            }
            else
            {
                checkResult = false;
                info.Add("Failed");
                return new ResultRecord(checkResult, GetType().Name, info.ToArray());
            }
        }

        private ResultRecord CompareVersionsAndReturnResult(bool checkResult, List<string> info, List<string> content)
        {
            if (string.IsNullOrEmpty(content.FirstOrDefault()))
                return new ResultRecord(false, GetType().Name, content.ToArray());

            var dbVersion = content[0];
            var exeVersion = StringifyVersion(_birokratExeVersion, dbVersion.Length);

            var major = new string(exeVersion.Take(exeVersion.Length - 3).ToArray());
            var minor = new string(exeVersion.Skip(Math.Max(0, exeVersion.Length - 3)).ToArray());

            info.Add("DatabaseVersion" + TextConstants.DELIMITER + dbVersion);
            info.Add("BirokratExeVersion" + TextConstants.DELIMITER + exeVersion);

            if (dbVersion.StartsWith(major))
            {
                info.Add($"Major version" + TextConstants.DELIMITER + TextConstants.POSTFIX_OK);
            }
            else
            {
                //For now this test will never return false since it can mess up execution. Untill we cant run birokrat via args automatically 
                checkResult = false;
                info.Add($"Major version" + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR);
            }

            if (dbVersion.EndsWith(minor))
                info.Add($"Minor version" + TextConstants.DELIMITER + TextConstants.POSTFIX_OK);
            else
                info.Add($"Minor version" + TextConstants.DELIMITER + TextConstants.POSTFIX_WARNING);

            return new ResultRecord(checkResult, GetType().Name, info.ToArray());
        }

        private string StringifyVersion(string version, int length)
        {
            var nodes = version.Split('.');
            for (int i = 1; i < nodes.Length; i++)
            {
                while (nodes[i].Length < 3)
                {
                    nodes[i] = "0" + nodes[i];
                }
            }

            var final = nodes.Aggregate((x, next) => x + next);

            var postfix = "";
            while (postfix.Length < length - final.Length)
                postfix += "0";

            return final + postfix;
        }
    }
}
