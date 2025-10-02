using System.Collections.Generic;

using tests_webshop.products;
using BiroWooHub.logic.integration;
using common_birowoo;
using tests.composition.root_builder;
using tests.interfaces;
using tests.composition.final_composers.tests;
using tests.composition.common;
using tests.composition.fixed_task.common;
using tests.tests.estrada;
using transfer_data_abstractions.orders;

namespace tests.composition.final_composers.production
{
    public class ProductionRootBuilder
    {

        ITaskExecutionStrategyFactory executor;
        IntegrationFactoryBuilder factoryBuilder;
        public ProductionRootBuilder(ITaskExecutionStrategyFactory factory, IntegrationFactoryBuilder factoryBuilder)
        {
            executor = factory;
            this.factoryBuilder = factoryBuilder;
        }

        public BirowooBootstraper Production(List<string> integrationNames,
            SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator,
            SimpleDecoratingFactory<IIntegration, IOrderTransferAccessor> orderDecorator)
        {

            var workloadObjectsBuilder = new ConstantTask_WorkloadObjectSourceBuilder();
            workloadObjectsBuilder.withLoggerFactory(new ListSaveLoggerFactory());
            workloadObjectsBuilder.withOutClientOverriding(debug: false, enforcedApiClient: null,
                                        enforceBiroToWoo: false, enforceWooToBiro: false);
            workloadObjectsBuilder.withIntegNames(integrationNames);
            workloadObjectsBuilder.withSource(WrapWithFactoriesHelper.Wrap(
                   new ProductionFactory(new transfer_data.system.OrderTransferSystemFactory(factoryBuilder.getSqlServer()), orderDecorator), productDecorator, null));
            workloadObjectsBuilder.AllowNoTestEnv();
            workloadObjectsBuilder.withIntegrationFactoryBuilder(factoryBuilder);
            return new BirowooBootstraper(workloadObjectsBuilder,
                    executor).AllowDangerous();
        }
    }

}
