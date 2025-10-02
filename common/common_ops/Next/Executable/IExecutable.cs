using System.Threading.Tasks;

namespace common_ops.Next.Executable
{
    public interface IExecutable
    {
        Task<(bool Result, string Message)> Execute();
    }
}
