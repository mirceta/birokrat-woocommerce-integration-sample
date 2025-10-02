using System;
using System.Collections.Generic;

namespace common_ops.diagnostics.Checks.General.Utils
{
    public interface IFileVersionExtractor
    {
        bool TryGetVersion(out Version version, string path);
    }
}
