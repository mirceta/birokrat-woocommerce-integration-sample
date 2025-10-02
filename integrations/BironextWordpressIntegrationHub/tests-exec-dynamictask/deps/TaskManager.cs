using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using ExecutionContext = tests.composition.common.ExecutionContext;
using System.Collections.Generic;
using System.Linq;

namespace tests_exec_dynamictask.deps
{
    public class TaskManager
    {
        private ConcurrentDictionary<string, LooperExecutionContext> contexts = new ConcurrentDictionary<string, LooperExecutionContext>();

        public async Task<Task> StartTask(ExecutionContext ctx)
        {

            CancellationTokenSource source = new CancellationTokenSource();

            // Using Task.Run to start the workload on a background thread
            var t = Task.Run(async () =>
            {
                try
                {
                    await ctx.WorkloadObject.Execute(source.Token);
                }
                // Additional logic could be added here
                finally
                {
                    //await informFinished(ctx);
                }
            });

            contexts[ctx.WorkloadObject.Signature] = new LooperExecutionContext
            {
                t = t,
                btwo = ctx,
                cancellationTokenSource = source
            };

            return t;
        }

        public async Task ReplaceTask(string name, ExecutionContext nwContext)
        {
            if (contexts.TryGetValue(name, out var existingContext))
            {
                existingContext.cancellationTokenSource.Cancel();
                await existingContext.t;
            }

            Task t = StartTask(nwContext);
        }

        public List<ExecutionContext> GetExecutionContexts()
        {
            return contexts.Values.Select(x => x.btwo).ToList();
        }
    }
}
