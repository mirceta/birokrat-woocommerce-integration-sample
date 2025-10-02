using BiroWoocommerceHubTests;
using System.Collections.Generic;
using tests.async;
using tests.tests.estrada;
using tests_webshop.products;
using webshop_client_woocommerce;
using BiroWooHub.logic.integration;
using common_birowoo;
using tests.composition.root_builder;
using System.Threading.Tasks;
using tests.interfaces;
using tests.composition.final_composers.validation;
using tests.composition.common;
using tests.composition.fixed_task.common;
using System;

namespace tests.composition.final_composers.tests
{
    public class TestsRootBuilder
    {

        ITaskExecutionStrategyFactory executor;
        Func<IOutApiClient> overridingClient;
        IntegrationFactoryBuilder factoryBuilder;

        public TestsRootBuilder(ITaskExecutionStrategyFactory factory,
            Func<IOutApiClient> overridingClient,
            IntegrationFactoryBuilder factoryBuilder)
        {
            executor = factory;
            this.overridingClient = overridingClient;
            this.factoryBuilder = factoryBuilder;
        }

        public BirowooBootstraper Tests(List<string> integrationNames,
            SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator,
            SimpleDecoratingFactory<IIntegration, IOutcomeHandler> orderDecoratorFactory,
            string integSettings)
        {

            /* 
            1. WOOTOBIRO TESTS
         RESPONSIBILITY: testing transfer of orders
             OPERATIONS: inserting production base orders into fake birokrat database, validation
                   BIRO: test
                WEBSHOP: prod
           UPLOADRESULT: local


            2. BIROTOWOO TESTS
         RESPONSIBILITY: check whether product transfer and sync works
             OPERATIONS: from test biro, upload to test site, change values randomly and ensure they are correctly synced.
                   BIRO: test
                WEBSHOP: test
           UPLOADRESULT: local
             */

            var workloadObjectsBuilder = new ConstantTask_WorkloadObjectSourceBuilder();
            workloadObjectsBuilder.withLoggerFactory(new ListSaveLoggerFactory());
            workloadObjectsBuilder.withOutClientOverriding(debug: true,
                        enforcedApiClient: overridingClient(),
                        enforceBiroToWoo: true, // we want to upload to fake site!
                        enforceWooToBiro: false); // we need to fetch orders from prod site!);
            workloadObjectsBuilder.withIntegNames(integrationNames);
            workloadObjectsBuilder.withSource(WrapWithFactoriesHelper.Wrap(
                   new OrderAndProductTestsFactory(), productDecorator, orderDecoratorFactory));
            workloadObjectsBuilder.withAdditionalParams(integSettings);
            workloadObjectsBuilder.withIntegrationFactoryBuilder(factoryBuilder);


            return new BirowooBootstraper(
                    workloadObjectsBuilder,
                    executor
                ).AllowDangerous();
        }
    }

    public class UploadProductionProductsDecorator : OutApiClientDecorator
    {
        public UploadProductionProductsDecorator(IOutApiClient next) : base(next) { }


        public override Task<Dictionary<string, object>> UpdateProduct(string id, Dictionary<string, object> values)
        {
            values["status"] = "publish";
            return base.UpdateProduct(id, values);
        }
        public override Task<Dictionary<string, object>> PostProduct(Dictionary<string, object> product)
        {
            product["status"] = "publish";
            return base.PostProduct(product);
        }

        public override Task<Dictionary<string, object>> PostBaseVariableProduct(Dictionary<string, object> product)
        {
            product["status"] = "publish";
            return base.PostBaseVariableProduct(product);
        }

        public override Task<Dictionary<string, object>> PostVariation(string parent_id, Dictionary<string, object> variation)
        {
            variation["status"] = "publish";
            return base.PostVariation(parent_id, variation);
        }

        public override Task<Dictionary<string, object>> UpdateVariation(string product_id, string variation_id, Dictionary<string, object> values)
        {
            values["status"] = "publish";
            return base.UpdateVariation(product_id, variation_id, values);
        }


    }

}
