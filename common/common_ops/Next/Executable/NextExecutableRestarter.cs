using common_ops.diagnostics.Utils;
using System;
using System.Threading.Tasks;

namespace common_ops.Next.Executable
{
    internal class NextExecutableRestarter : IExecutable
    {
        private readonly IExecutable _nextExecutableKiller;
        private readonly ProcessLauncher _launcher;
        private readonly string _pathToNextExe;

        public NextExecutableRestarter(ProcessLauncher launcher, string pathToNextExe, IExecutable nextExecutableKiller = null)
        {
            _launcher = launcher;
            _pathToNextExe = pathToNextExe;
            _nextExecutableKiller = nextExecutableKiller;
        }

        public async Task<(bool Result, string Message)> Execute()
        {
            if (_nextExecutableKiller != null)
            {
                var result = await _nextExecutableKiller.Execute();
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            var startResult = await _launcher.Start_DetachedProcessAsync(_pathToNextExe);
            await Task.Delay(TimeSpan.FromSeconds(10)); // to give next time to set up
            return (true, Environment.NewLine + startResult);
        }
    }
}
