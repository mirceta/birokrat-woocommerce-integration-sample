using core.customers;
using tests.tests.estrada;
using si.birokrat.next.common.logging;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace tests.interfaces
{
    public interface ITestsFactory
    {
        ITests<string> GetTests(LazyIntegration integ, TestEnvironmentParams testenv, IMyLogger logger, object additionalParams);
    }
    public class TestFactory : ITestsFactory
    {
        Func<LazyIntegration, TestEnvironmentParams, IMyLogger, object, ITests<string>> action;
        public TestFactory(Func<LazyIntegration, TestEnvironmentParams, IMyLogger, object, ITests<string>> action)
        {
            this.action = action;
        }

        public ITests<string> GetTests(LazyIntegration integ, TestEnvironmentParams testenv, IMyLogger logger, object additionalParams)
        {
            return action(integ, testenv, logger, additionalParams);
        }
    }

    public class Tests : ITests<string>
    {

        Func<CancellationToken, Task> work;
        Func<string> result;
        public Tests(Func<CancellationToken, Task> work, Func<string> result)
        {
            this.work = work;
            this.result = result;
        }

        public string GetResult()
        {
            return result();
        }

        public async Task Work(CancellationToken token)
        {
            await work(token);
        }
    }
}
