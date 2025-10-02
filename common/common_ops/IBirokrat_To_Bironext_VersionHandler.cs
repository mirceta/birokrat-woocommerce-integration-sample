using System;

namespace common_ops
{
    public interface IBirokrat_To_Bironext_VersionHandler
    {
        string GetBironextVersion(string bironextDeployFolder = "");
        void Verify_CBirokrat_And_Bironext_VersionsAreSame(Action<string> logfunc, string bironextDeployFolder = "");
    }
}