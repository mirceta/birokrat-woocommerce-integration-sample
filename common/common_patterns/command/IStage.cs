using System;
using System.Collections.Generic;
using System.Text;

namespace common_patterns.command
{
    public interface IStage
    {
        void Execute();
        string Name();
    }
}
