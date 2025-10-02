using common_ops.diagnostics.Utils;
using common_ops.Executors.Shell;
using common_ops.Next.Executable;

namespace common_ops.Next
{
    public class ExecutableFactory
    {
        public IExecutable BuildNextRestarter(string runnerGlobalExe, params int[] ports)
        {
            return new NextExecutableRestarter(
                new ProcessLauncher(),
                runnerGlobalExe,
                BuildNextKiller(ports));
        }

        public IExecutable BuildNextRestarter_WithoutKill(string runnerGlobalExe, params int[] ports)
        {
            return new NextExecutableRestarter(
                new ProcessLauncher(),
                runnerGlobalExe);
        }

        public IExecutable BuildNextKiller(params int[] ports)
        {
            return new NextExecutableKiller(new ShellExecutor(), ports);
        }

        public IExecutable BuildBirokratKiller()
        {
            return new BirokratExecutableKiller(new ShellExecutor());
        }
    }
}
