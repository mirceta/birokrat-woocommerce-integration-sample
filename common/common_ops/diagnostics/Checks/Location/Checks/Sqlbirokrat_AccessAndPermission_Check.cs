using common_ops.diagnostics.Checks.Location.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Location.Checks
{
    /// <summary>
    /// Will check if sqlbirokrat has ReadAndWrite permission.
    /// Result is determined if permissions is true.
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: path_to_location||result for permission check.
    /// categories are separated with <c>||</c>. Example:</para>
    /// <para>\\\\sqlbirokrat\\Andersen||true</para>
    /// </summary>
    ///
    public class Sqlbirokrat_AccessAndPermission_Check : ICheck
    {
        private readonly ILocationHelper _locationHelper;

        /// <summary>
        /// <inheritdoc cref="Sqlbirokrat_AccessAndPermission_Check"/>
        /// </summary>
        public Sqlbirokrat_AccessAndPermission_Check(ILocationHelper locationHelper)
        {
            _locationHelper = locationHelper;
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

        public Task<ResultRecord> Work()
        {
            var result = _locationHelper.IsWritePermissionGranted(BiroLocationConstants.DefaultSqlbirokratLocation);

            var info = BiroLocationConstants.DefaultSqlbirokratLocation + TextConstants.DELIMITER + result;
            var record = new ResultRecord(result, GetType().Name, info);

            return Task.FromResult(record);
        }
    }
}
