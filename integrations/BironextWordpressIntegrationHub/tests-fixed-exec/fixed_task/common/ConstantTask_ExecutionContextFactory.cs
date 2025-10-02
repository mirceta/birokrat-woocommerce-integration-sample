using System.Collections.Generic;
using tests.tests.estrada;
using tests.async;
using System.Linq;
using core.customers;
using si.birokrat.next.common.logging;
using tests.interfaces;
using tests.composition.common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace tests.composition.fixed_task.common
{
    public class ConstantTask_ExecutionContextFactory
    {
        private ITestsFactory source;
        private TestEnvironmentParams testenv;
        string additionalParams;

        public ConstantTask_ExecutionContextFactory(ITestsFactory source, 
            TestEnvironmentParams testenv,
            string additionalParams)
        {
            this.source = source;
            this.testenv = testenv;
            this.additionalParams = additionalParams;
        }

        public List<ExecutionContext> CreateExecutionContexts(List<LazyIntegration> integrations, Dictionary<string, IMyLogger> loggers)
        {
            var workloadObjects = integrations.Select(x => new BirowooTestWorkloadObj(
                source.GetTests(x, testenv, loggers[x.Name], additionalParams), 
                x.Name)
            ).ToList();

            var executionContexts = new List<ExecutionContext>();
            for (int i = 0; i < integrations.Count; i++)
            {
                executionContexts.Add(new ExecutionContext(loggers[integrations[i].Name], workloadObjects[i], integrations[i].Name));
            }

            return executionContexts;
        }
    }
}
