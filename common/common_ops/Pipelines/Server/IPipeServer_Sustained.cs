using System;

namespace common_ops.PipeLines.Server
{
    public interface IPipeServer_Sustained : IDisposable
    {
        bool IsActive { get; }
    }
}
