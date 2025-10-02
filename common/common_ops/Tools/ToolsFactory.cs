using common_ops.Executors.Shell;
using System;

namespace common_ops.Tools
{
    public class ToolsFactory
    {
        public RTClibRegistryRemover Build_RTClibRegistryRemover()
        {
            return new RTClibRegistryRemover(new ShellExecutor());
        }
    }
}
