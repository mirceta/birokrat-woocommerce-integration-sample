using System;

namespace common_ops.Next.Utils
{
    public interface IFileVersionHelper
    {
        Version GetFileVersion(string executablePath);
        string GetLatestDeploymentFromSqlbirokrat(string sqlBirokratDeplomentFolder);
        string GetLatestDeploymentFromSqlbirokrat_CompareToLocalVersion(string sqlBirokratDeplomentFolder, string localLatestFolder);
    }
}
