using core.customers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using tests.tests.estrada;
using apirest;
using BiroWooHub.logic.integration;
using biro_to_woo.logic.change_trackers.exhaustive;
using Microsoft.Extensions.Logging;
using System.Threading;
using ExecutionContext = tests.composition.common.ExecutionContext;
using tests.interfaces;
using common_multithreading;



/*
 Story:
Problem importance reasoning:
Get bironext server under control. Bironext server and birokrat integrations are actually strongly related.
BIROWOO TESTS are tests for bironext server as well as birowoo project.
Birowoo tests should be faking metalno dobro odradjeno, because it is the most important project.
So BIROWOO TESTS ARE THE MOST IMPORTANT GOAL.
Problem specification:
For parameter list List<integration name> find all integrations they have.
Concurrently execute all of the tests for these firms.
Generate comprehensive reports on the tests so that you know exactly what works and what doesn’t.
Generate reports on how many times bironext was restarted and what errors occurred during the time bironext was restarted.
Problem dissection:
Task 1: If failed order/product retry {PARAMETER} times, if still fail, then restart bironext! - this should be encapsulated into a failure handling class which is injected into relevant mechanisms.
If tests are running async for multiple firms - this needs to be synchronized.
Task 2:
The tests should run asynchronously.
This is rather simple, as in this case the tests are not looping but rather only executed one time. Thus you can just start a new thread and run it - the only shared resources are BironextResetter and DatabaseResetter.
Task 3:
Need another top level abstraction - MultipleTestDriver, SingleTestDriver.
MultipleTestDriver(List<IIntegration> integrations) { // can be both!
SingleTestDriver(IIntegration integration) // if BIROTOWOO … product tests else ordertests
Task 3:
For products: webshop must be either mocked, or we have multiple woocommerces.
Task 4: 
Test report generation: how do we know that we pass?

 */

namespace tests.composition.fixed_integration.fixed_task.task_execution_strategy
{
    public class MultithreadedTaskExecutionStrategyFactory : ITaskExecutionStrategyFactory
    {
        public async Task<ITaskExecutionStrategy> Create(IWorkloadObjectSource workloadObjects)
        {
            var some = await workloadObjects.Get();
            return new Multithreaded(some);
        }
    }

    public class Multithreaded_ForeverLoopTasks_TaskExecutionStrategyFactory : ITaskExecutionStrategyFactory
    {
        public async Task<ITaskExecutionStrategy> Create(IWorkloadObjectSource workloadObjects)
        {
            var some = await workloadObjects.Get();
            return new Multithreaded(some, loopTaskForever: true);
        }
    }

    public class Multithreaded : ITaskExecutionStrategy
    {

        List<ExecutionContext> integrations;
        AllFinishedObservable allFinished;


        private SemaphoreQueue semaphore; // used to be SemaphoreSlim
        private const int MaxConcurrentTasks = 6;
        bool loopTaskForever;

        public Multithreaded(List<ExecutionContext> integrations, bool loopTaskForever = false)
        {
            this.loopTaskForever = loopTaskForever;
            this.integrations = integrations;
            allFinished = new AllFinishedObservable(integrations.Select(e => e.WorkloadObject).ToList());
            semaphore = new SemaphoreQueue(MaxConcurrentTasks, MaxConcurrentTasks);
        }

        public async Task<AllFinishedObservable> Run()
        {

            List<Task> tasks = new List<Task>();
            foreach (ExecutionContext ctx in integrations)
            {
                Task t = new Task(async () =>
                {
                    if (!loopTaskForever)
                    {
                        await executeTask(ctx);
                    }
                    else
                    {
                        while (true)
                        {
                            await executeTask(ctx);
                        }
                    }

                });
                tasks.Add(t);
            }

            foreach (Task t in tasks)
            {
                t.Start();
            }

            return allFinished;
        }

        private async Task executeTask(ExecutionContext ctx)
        {
            await waitAtSemaphore(ctx);
            try
            {
                await ctx.WorkloadObject.Execute(new CancellationTokenSource().Token);
            }
            finally
            {
                await informFinished(ctx.WorkloadObject);
                semaphore.Release();
                ctx.WorkloadObject.NotifyExternalEvent("operational", "Exited semaphore...");
            }
        }

        private async Task waitAtSemaphore(ExecutionContext ctx)
        {
            using (var timer = new System.Timers.Timer(30000))
            {

                ctx.WorkloadObject.NotifyExternalEvent("operational", "waiting");
                timer.Elapsed += (sender, e) =>
                {
                    ctx.WorkloadObject.NotifyExternalEvent("operational", "waiting");
                };
                timer.Start();

                await semaphore.WaitAsync(); // Wait for a slot to be free
                timer.Stop(); // Stop the timer once the task proceeds

                ctx.WorkloadObject.NotifyExternalEvent("operational", "entering");
            }
        }

        public Task Update(IUpdateCommand command)
        {
            throw new NotImplementedException();
        }


        int finishedCount = 0;
        private async Task informFinished(IWorkloadObj<string> finishee)
        {
            Interlocked.Increment(ref finishedCount);
            if (finishedCount == integrations.Count)
            {
                await allFinished.Notify();
            }
        }

        public AllFinishedObservable GetObservable()
        {
            return allFinished;
        }

        public async Task<AllFinishedObservable> GetAllFinishedObservable()
        {
            return allFinished;
        }

        public List<ExecutionContext> GetExecutionContexts()
        {
            return integrations;
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            return new Notifier().Subscribe(observer);
        }
    }


    // MOCK CLASS - USELESS
    public class Notifier : IObservable<string>
    {
        private List<IObserver<string>> observers = new List<IObserver<string>>();

        public void Notify(string change)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(change);
            }
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            return new Unsubscriber(observers, observer);
        }
    }
    // MOCK CLASS - USELESS
    class Unsubscriber : IDisposable
    {
        private List<IObserver<string>> _observers;
        private IObserver<string> _observer;

        public Unsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
