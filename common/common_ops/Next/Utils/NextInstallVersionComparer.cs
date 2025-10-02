using System.Collections.Generic;
using System.Linq;

namespace common_ops.Next.Utils
{
    internal class NextInstallVersionComparer
    {
        internal readonly List<RunnerGlobalInfo> _versions;

        public NextInstallVersionComparer()
        {
            _versions = new List<RunnerGlobalInfo>();
        }

        public void CheckVersions(RunnerGlobalInfo localNextInfo, RunnerGlobalInfo sqlBirokratNextInfo)
        {
            var result = sqlBirokratNextInfo.ProductVersion.CompareTo(localNextInfo.ProductVersion);

            if (result > 0)
            {
                //sqlBirokratNextInfo is newer version
                _versions.Add(sqlBirokratNextInfo);
            }
            if (result == 0)
            {
                if (sqlBirokratNextInfo.ModifiedDate.CompareTo(localNextInfo.ModifiedDate) > 0)
                {
                    //sqlBirokratNextInfo is same version but has newer creation date
                    _versions.Add(sqlBirokratNextInfo);
                }
            }
        }

        internal string GetLatestVersion()
        {
            if (_versions.Count == 0)
                return string.Empty;

            var sortedList = _versions
                .OrderBy(x => x.ProductVersion)
                .ThenBy(x => x.ModifiedDate)
                .Reverse()
                .ToList();

            return sortedList.FirstOrDefault().RootPath;
        }
    }
}
