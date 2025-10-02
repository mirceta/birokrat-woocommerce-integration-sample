using apirest;
using core.customers;
using si.birokrat.next.common.logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tests.composition.fixed_task.common;

namespace tests.composition.fixed_integration.fixed_task.fixed_workload_object_source.deps
{
    public class IntegrationService
    {
        private IIntegrationFactory factory;
        private LoggerService loggerService;

        public IntegrationService(IntegrationFactoryBuilder integrationFactoryFactory,
                                  string bironextAddress,
                                  string integrationDataFolder,
                                  LoggerService loggerService)
        {
            factory = integrationFactoryFactory.build(bironextAddress, integrationDataFolder);
            this.loggerService = loggerService;
        }


        public async Task<List<LazyIntegration>> GetIntegrations(List<string> integNames, Dictionary<string, IMyLogger> loggers)
        {
            var lazyIntegrations = await factory.GetAllLazy();
            var integrations = lazyIntegrations.Where(x => integNames.Contains(x.Name)).ToList();
            integrations.ForEach(x => x.Logger = loggers[x.Name]);
            return integrations;
        }
    }
}
