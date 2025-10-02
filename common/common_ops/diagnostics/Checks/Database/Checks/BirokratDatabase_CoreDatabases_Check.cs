using common_ops.diagnostics.Checks.Database.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Database.Checks
{
    /// <summary>
    /// Checks for core databases on server (application, biromaster and configuration)
    /// Results are determined based on whether any databases are found. Will return false if any of core databases is not present
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: databaseName or in case of an error databaseName||null. Separated with <c>||</c> </para>
    /// </summary>
    public class BirokratDatabase_CoreDatabases_Check : ICheck
    {
        private readonly IBirokratQueryExecutor _biroQueryExecutor;
        private readonly string _connectionString;
        private readonly string[] REQUIRED = { "application", "biromaster", "configuration" };

        /// <summary>
        /// <inheritdoc cref="BirokratDatabase_CoreDatabases_Check"/>
        /// </summary>
        public BirokratDatabase_CoreDatabases_Check(IBirokratQueryExecutor biroQueryExecutor, string connectionString)
        {
            _biroQueryExecutor = biroQueryExecutor;
            _connectionString = connectionString;
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
            var coreDbs = await _biroQueryExecutor.GetCoreDatabasesAsync(_connectionString);
            var check = true;
            List<string> output = new List<string>();

            if (coreDbs.Count == 0)
                return new ResultRecord(false, GetType().Name, output.ToArray());

            foreach (var req in REQUIRED)
            {

                if (!coreDbs.Any(x => x.Equals(req, StringComparison.CurrentCultureIgnoreCase)))
                {
                    output.Add($"{req}{TextConstants.DELIMITER}{TextConstants.NULL_FIELD}");
                    check = false;
                }
                else
                {
                    output.Add($"{req}");
                }
            }
            return new ResultRecord(check, GetType().Name, output.ToArray());
        }
    }
}
