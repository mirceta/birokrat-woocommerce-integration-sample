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

namespace tests.composition.fixed_integration.fixed_task.fixed_workload_object_source
{
    public class FixedWorkloadObjectSource : IWorkloadObjectSource
    {
        private LoggerService loggerService;
        private ITestsFactory source;
        private TestEnvironmentParams testenv;
        private IntegrationService integrationService;
        private Dictionary<string, IMyLogger> loggers = new Dictionary<string, IMyLogger>();

        List<string> integNames;

        ConstantTask_ExecutionContextFactory executionContextFactory;


        IntegrationFactoryBuilder integrationFactoryFactory;
        string bironextAddress;
        string integrationDataFolder;


        public FixedWorkloadObjectSource(IMyLoggerFactory loggerFactory,
                                  ITestsFactory source,
                                  TestEnvironmentParams testenv,
                                  IntegrationFactoryBuilder integrationFactoryFactory,
                                  string bironextAddress,
                                  string integrationDataFolder,
                                  List<string> integNames,
                                  string additionalParams)
        {
            loggerService = new LoggerService(loggerFactory);
            executionContextFactory = new ConstantTask_ExecutionContextFactory(source, testenv, additionalParams);
            this.integNames = integNames;
            this.bironextAddress = bironextAddress;
            this.integrationDataFolder = integrationDataFolder;
            this.integrationFactoryFactory = integrationFactoryFactory;
            this.source = source;
            this.testenv = testenv;
            
        }

        public Task<List<string>> DetectChanges()
        {
            throw new NotImplementedException();
        }

        public async Task<List<ExecutionContext>> Get()
        {
            var loggers = loggerService.CreateLoggers(integNames);
            var factory = integrationFactoryFactory.build(bironextAddress, integrationDataFolder);
            var lazyIntegration = await factory.GetAllLazy();
            var integrations = lazyIntegration.Where(x => integNames.Contains(x.Name)).ToList();
            integrations.ForEach(x => x.Logger = loggers[x.Name]);
            return executionContextFactory.CreateExecutionContexts(integrations, loggers);
        }

        public async Task<ExecutionContext> Get(string signature)
        {
            throw new NotImplementedException();
        }
    }

}
