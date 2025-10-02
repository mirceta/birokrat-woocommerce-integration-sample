using common_ops.Executors.Shell;
using common_ops.FileHandler;
using common_ops.Next.Executable;
using common_ops.Next.Installation;
using System;

namespace common_ops.NextInstallation
{
    public class NextInstallerFactory
    {
        public INextInstaller Build(Action<string> logger, string nextLocation, bool overwrite = false)
        {
            return new NextInstaller(
                    logger,
                    new DirectoryContentHandlerFactory().Build(logger),
                    new NextExecutableKiller(new ShellExecutor()),
                    new FileBackup(false),
                    overwrite);
        }
    }
}
