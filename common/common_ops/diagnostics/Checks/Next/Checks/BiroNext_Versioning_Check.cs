using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Next.Checks
{
    public class BiroNext_Versioning_Check : ICheck
    {
        private readonly IBirokrat_To_Bironext_VersionHandler _versionHandler;
        private readonly string _biroNextLocation;

        public BiroNext_Versioning_Check(IBirokrat_To_Bironext_VersionHandler versionHandler, string biroNextLocation)
        {
            _versionHandler = versionHandler;
            _biroNextLocation = biroNextLocation;
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
            var info = new List<string>();
            Action<string> log = (message) => info.Add(message);

            _versionHandler.Verify_CBirokrat_And_Bironext_VersionsAreSame(log, _biroNextLocation);

            var ok = "The versions match";

            ResultRecord record;
            if (info.FirstOrDefault().StartsWith(ok, StringComparison.OrdinalIgnoreCase))
                record = new ResultRecord(true, GetType().Name, info.ToArray());
            else
                record = new ResultRecord(false, GetType().Name, info.ToArray());
            return record;
        }
    }
}
