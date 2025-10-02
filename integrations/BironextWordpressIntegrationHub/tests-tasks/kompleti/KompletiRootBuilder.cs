using BiroWooHub.logic.integration;
using common_birowoo;
using core.customers;
using products_to_excel;
using si.birokrat.next.common.logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using tests.composition.final_composers;
using tests.composition.final_composers.tests;
using tests.composition.fixed_task.common;
using tests.composition.root_builder;
using tests.interfaces;
using tests.tests.estrada;
using tests_webshop.products;

namespace tests_tasks.kompleti
{
    public class KompletiRootBuilder
    {
        ITaskExecutionStrategyFactory executor;
        IntegrationFactoryBuilder factoryBuilder;
        public KompletiRootBuilder(ITaskExecutionStrategyFactory factory, IntegrationFactoryBuilder factoryBuilder)
        {
            executor = factory;
            this.factoryBuilder = factoryBuilder;
        }

        public BirowooBootstraper Kompleti(string integrationName)
        {
            var workloadObjectsBuilder = new ConstantTask_WorkloadObjectSourceBuilder();
            workloadObjectsBuilder.withLoggerFactory(new ListSaveLoggerFactory());
            workloadObjectsBuilder.withOutClientOverriding(debug: true,
                        enforcedApiClient: null,
                        enforceBiroToWoo: false, // only this relevant.
                        enforceWooToBiro: false);
            workloadObjectsBuilder.withIntegNames(new List<string> { integrationName });
            workloadObjectsBuilder.withSource(
                WrapWithFactoriesHelper.Wrap(
                    new KompletiFactory(), null, null));
            workloadObjectsBuilder.AllowNoTestEnv();
            workloadObjectsBuilder.withIntegrationFactoryBuilder(factoryBuilder);
            return new BirowooBootstraper(workloadObjectsBuilder,
                    executor).AllowDangerous();
        }
    }
    public class KompletiFactory : IActualWorkFactory
    {
        public IActualWork Create()
        {
            return new Kompleti();
        }
    }
    public class Kompleti : IActualWork
    {
        public Kompleti()
        {
        }
        public object AdditionalParams { get; set; }
        public string Result => "";

        public async Task Tests(SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator,
            SimpleDecoratingFactory<IIntegration, IOutcomeHandler> orderDecoratorFactory,
            LazyIntegration lazyIntegration,
            TestEnvironmentParams testenv,
            IMyLogger logger,
            CancellationToken cancellationToken)
        {
            var integration = await lazyIntegration.BuildIntegrationAsync.Invoke();

            var generator = new KompletiGenerator(integration, false, logger);
            await generator.Execute();
        }
    }
}
