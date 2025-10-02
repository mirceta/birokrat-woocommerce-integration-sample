using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using ExecutionContext = tests.composition.common.ExecutionContext;
using System.Threading;
using tests.interfaces;
using tests.composition.fixed_integration.fixed_task.task_execution_strategy;
using tests_exec_dynamictask.deps;

namespace tests_exec_dynamictask
{

    public class LooperFactory : ITaskExecutionStrategyFactory
    {

        public async Task<ITaskExecutionStrategy> Create(IWorkloadObjectSource workloadObjectSource)
        {
            return new Looper(workloadObjectSource);
        }
    }

    public class Looper : ITaskExecutionStrategy, IObservable<string>
    {
        IWorkloadObjectSource source;
        Notifier notifier;
        TaskManager taskManager;
        CancellationTokenSource cancellationTokenSource;  // Add this

        public Looper(IWorkloadObjectSource source)
        {
            this.source = source;
            notifier = new Notifier();
            taskManager = new TaskManager();
            cancellationTokenSource = new CancellationTokenSource();  // Initialize it
        }

        public async Task<AllFinishedObservable> Run()
        {
            var integrations = await source.Get();
            foreach (var ctx in integrations)
            {
                await taskManager.StartTask(ctx);
            }

            // Use Task.Run to start the forever loop in the background
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(5000, cancellationTokenSource.Token);  // Add token here
                        var changes = await source.DetectChanges();
                        foreach (var change in changes)
                        {
                            var nw = await source.Get(change);
                            await taskManager.ReplaceTask(change, nw);
                            notifier.Notify(change);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            });

            // placeholder
            return new AllFinishedObservable(null);
        }

        public void Stop()  // This method allows for the cancellation of the background task
        {
            cancellationTokenSource.Cancel();
        }
        public Task<AllFinishedObservable> GetAllFinishedObservable()
        {
            // placeholder
            return Task.FromResult(new AllFinishedObservable(null));
        }

        public List<ExecutionContext> GetExecutionContexts()
        {
            return taskManager.GetExecutionContexts();
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            return notifier.Subscribe(observer);
        }

        public Task Update(IUpdateCommand command)
        {
            // restart command
            // add command
            // stop command
            throw new NotImplementedException();
        }
    }
}
