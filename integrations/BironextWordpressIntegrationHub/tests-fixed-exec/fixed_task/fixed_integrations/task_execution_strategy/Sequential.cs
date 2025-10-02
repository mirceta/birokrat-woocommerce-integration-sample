using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using tests.async;
using tests.interfaces;



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

    public class Sequential_TaskExecutionStrategyFactory : ITaskExecutionStrategyFactory
    {
        public async Task<ITaskExecutionStrategy> Create(IWorkloadObjectSource workloadObjects)
        {
            var some = (await workloadObjects.Get())[0].WorkloadObject;
            return new Sequential(some);
        }
    }
    public class Sequential : ITaskExecutionStrategy
    {

        BirowooTestWorkloadObj integrationContext;

        public Sequential(BirowooTestWorkloadObj integrationContext)
        {
            this.integrationContext = integrationContext;
        }

        public Task<AllFinishedObservable> GetAllFinishedObservable()
        {
            throw new NotImplementedException();
        }

        public List<common.ExecutionContext> GetExecutionContexts()
        {
            throw new NotImplementedException();
        }

        public async Task<AllFinishedObservable> Run()
        {
            await integrationContext.Execute(new CancellationTokenSource().Token);
            return null;
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            throw new NotImplementedException();
        }

        public Task Update(IUpdateCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
