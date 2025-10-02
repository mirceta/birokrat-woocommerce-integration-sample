using System.Threading.Tasks;

namespace common_ops.Next.Installation
{
    public interface INextInstaller
    {
        Task<bool> TransferToArchive(string source, string deploymentName);
        Task<bool> CreateBackupExe(string deploymentName);
        Task<bool> TransferToLatest(string deploymentName);
        string GetPreviousDeployment(string sourceDirectory);
    }
}
