using biro_to_woo_common.executor.validation;
using BiroWoocommerceHubTests;
using core.customers;
using System.Collections.Generic;
using tests.tests.estrada;
using tests.tests.hisavizij;
using tests_webshop.products;
using biro_to_woo.loop;
using BiroWooHub.logic.integration;
using common_birowoo;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using tests.composition.root_builder;
using Microsoft.Extensions.Logging;
using System.CodeDom.Compiler;
using webshop_client_woocommerce;
using tests.interfaces;
using tests.composition.final_composers.production;
using tests.composition.final_composers.validation;
using tests.composition.final_composers.tests;
using tests.composition.common;
using tests.composition.fixed_task.common;
using si.birokrat.next.common.database;
using gui_generator_integs.final_adapter;
using administration_data.data.structs;
using tests.composition.final_composers;
using System.Threading.Tasks;
using System;
using transfer_data_abstractions.orders;

namespace tests
{

    public partial class LooperTestsFactory
    {

        ITaskExecutionStrategyFactory executor;
        Func<IOutApiClient> overridingClient;
        public LooperTestsFactory(ITaskExecutionStrategyFactory factory,
                                  Func<IOutApiClient> overridingClientFactory)
        {
            executor = factory;
            this.overridingClient = overridingClientFactory;
        }


        public BirowooBootstraper LooperTests(SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator,
            SimpleDecoratingFactory<IIntegration, IOutcomeHandler> orderDecoratorFactory)
        {

            var workloadObjectsBuilder = new ConstantTask_WorkloadObjectSourceBuilder();
            workloadObjectsBuilder.withLoggerFactory(new ListSaveLoggerFactory());
            workloadObjectsBuilder
                            .withOutClientOverriding(debug: true,
                                                    enforcedApiClient: overridingClient(),
                                                    enforceBiroToWoo: true, // we want to upload to fake site!
                                                    enforceWooToBiro: false) // we need to fetch orders from prod site!);
                            .withSource(WrapWithFactoriesHelper.Wrap(
                                new OrderAndProductTestsFactory(), productDecorator, orderDecoratorFactory));


            return new BirowooBootstraper(
                    workloadObjectsBuilder,
                    executor
                ).AllowDangerous();
        }
    }
}
