using System;
using System.Threading.Tasks;

namespace common_ops.PipeLines.Server
{
    public interface IPipeServer_Single : IDisposable
    {
        bool IsActive { get; }
        Task WaitForMessageAsync();
    }
}
