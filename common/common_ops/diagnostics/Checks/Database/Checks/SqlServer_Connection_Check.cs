using common_ops.diagnostics.Constants;
using common_ops.Executors.Sql;
using System;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Database.Checks
{
    /// <summary>
    /// Checks whether connection to SqlServer can be established. Will return false if connection cant be established. 
    /// For example when wrong sql server name is provided in connection string
    /// 
    /// <para><see cref="ResultRecord.AdditionalInfo"/>will always be empty. Only look at <see cref="ResultRecord.Result"/></para>
    /// </summary>
    public class SqlServer_Connection_Check : ICheck
    {
        private readonly ISqlUtils _sqlUtils;
        private readonly string _connectionString;

        /// <summary>
        /// <inheritdoc cref="SqlServer_Connection_Check"/>
        /// </summary>
        public SqlServer_Connection_Check(ISqlUtils sqlUtils, string connectionString)
        {
            _sqlUtils = sqlUtils;
            _connectionString = connectionString;
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                var result = await _sqlUtils.CheckSqlServer(_connectionString);
                return new ResultRecord(result, GetType().Name, string.Empty);
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
        }
    }
}
