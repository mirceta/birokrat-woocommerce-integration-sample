using core.customers;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tests.composition.common;
using tests.composition.fixed_integration.fixed_task.fixed_workload_object_source.deps;
using tests.composition.fixed_task.common;
using tests.composition.root_builder;
using tests.interfaces;
using tests.tests.estrada;

namespace tests.composition.fixed_integration.looper
{
    public class LooperWorkloadObjectSource : IWorkloadObjectSource
    {
        private LoggerService loggerService;
        ConstantTask_ExecutionContextFactory executionContextFactory;
        SqlIntegrationFactory sqlIntegrationFactory;
        IVersionPicker versionPicker;

        public LooperWorkloadObjectSource(IMyLoggerFactory loggerFactory,
                                  ITestsFactory source,
                                  TestEnvironmentParams testenv,
                                  SqlIntegrationFactory sqlIntegrationFactory,
                                  IVersionPicker versionPicker)
        {
            loggerService = new LoggerService(loggerFactory);
            executionContextFactory = new ConstantTask_ExecutionContextFactory(source, testenv, null);

            this.sqlIntegrationFactory = sqlIntegrationFactory;
            this.versionPicker = versionPicker;
        }

        DateTime lastChecked = DateTime.MinValue;
        public async Task<List<string>> DetectChanges()
        {
            var result = versionPicker.DetectChanges(lastChecked);
            lastChecked = DateTime.UtcNow;
            return result;
        }

        public async Task<List<ExecutionContext>> Get()
        {
            var lazies = sqlIntegrationFactory.GetAllLazy();
            var lazyList = await lazies;
            var integNames = lazyList.Select(x => x.Name).ToList();
            var loggers = loggerService.CreateLoggers(integNames);
            lazyList.ForEach(x => x.Logger = loggers[x.Name]);
            return executionContextFactory.CreateExecutionContexts(lazyList, loggers);
        }

        public async Task<ExecutionContext> Get(string signature)
        {
            var lazy = sqlIntegrationFactory.GetLazy(signature);
            var lazyIntegration = await lazy;

            var logger = loggerService.CreateLoggers(new List<string> { lazyIntegration.Name });
            lazyIntegration.Logger = logger[lazyIntegration.Name];
            return executionContextFactory.CreateExecutionContexts(
                new List<LazyIntegration> { lazyIntegration },
                new Dictionary<string, IMyLogger> {
                    { lazyIntegration.Name, lazyIntegration.Logger }
                }
            )[0];
        }
    }

}
