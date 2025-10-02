using biro_to_woo_common.executor;
using biro_to_woo_common.executor.context_processor;
using BiroWooHub.logic.integration;
using common_birowoo;
using core.customers;
using si.birokrat.next.common.logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tests.composition.final_composers.tests;
using tests.composition.fixed_task.common;
using tests.composition.root_builder;
using tests.interfaces;
using tests.tests.estrada;
using tests.tests.hisavizij;
using tests_webshop.products;
using transfer_data.products;

namespace tests.composition.final_composers.validation
{
    public class ProductValidationRootBuilder
    {
        ITaskExecutionStrategyFactory executor;
        IntegrationFactoryBuilder factoryBuilder;
        public ProductValidationRootBuilder(ITaskExecutionStrategyFactory factory, IntegrationFactoryBuilder factoryBuilder)
        {
            executor = factory;
            this.factoryBuilder = factoryBuilder;
        }

        public BirowooBootstraper ProductValidation(List<string> integrationNames, bool uploadResultToProduction,
            SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator)
        {
            /*
            RESPONSIBILITY: checking database compliance and reporting errors locally
            OPERATIONS: database compliance check (no change detection, no synchronization, no uploading)
                 BIRO: prod
              WEBSHOP: prod
               RESULT: $uploadResultToProduction
           */

            // TODO: Add decorator same way as production

            var workloadObjectsBuilder = new ConstantTask_WorkloadObjectSourceBuilder();
            workloadObjectsBuilder.withLoggerFactory(new ListSaveLoggerFactory());
            workloadObjectsBuilder.withOutClientOverriding(debug: true,
                        enforcedApiClient: null,
                        enforceBiroToWoo: false, // only this relevant.
                        enforceWooToBiro: false);
            workloadObjectsBuilder.withIntegNames(integrationNames);
            workloadObjectsBuilder.withSource(
                WrapWithFactoriesHelper.Wrap(
                    new ProductValidationFactory(uploadResultToProduction), productDecorator, null));
            workloadObjectsBuilder.AllowNoTestEnv();
            workloadObjectsBuilder.withIntegrationFactoryBuilder(factoryBuilder);

            return new BirowooBootstraper(workloadObjectsBuilder,
                    executor).AllowDangerous();
        }
    }

    public class ProductValidationFactory : IActualWorkFactory
    {
        bool uploadResultToProduction;

        public ProductValidationFactory(bool uploadProductToProduction)
        {
            this.uploadResultToProduction = uploadProductToProduction;
        }

        public IActualWork Create()
        {
            return new ProductValidation(uploadResultToProduction);
        }
    }

    public class ProductValidation : IActualWork
    {

        bool uploadResultToProduction;
        public ProductValidation(bool uploadResultToProduction)
        {
            this.uploadResultToProduction = uploadResultToProduction;
        }

        public object AdditionalParams { get; set; }

        public string Result { get {
                if (hdlr != null)
                {
                    var reports = hdlr.GetReports();
                    return string.Join("\n\n\n", reports.Select(x => x.ToString()));
                }
                else {
                    return "Not finished yet!";
                }
        } }


        TestErrorHandler hdlr = null;
        public async Task Tests(SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator,
            SimpleDecoratingFactory<IIntegration, IOutcomeHandler> orderDecoratorFactory,
            LazyIntegration lazyIntegration,
            TestEnvironmentParams testenv,
            IMyLogger logger,
            CancellationToken cancellationToken)
        {
            var testset = new SimpleComparisonContextCreator();
            var integration = await lazyIntegration.BuildIntegrationAsync();

            IProductTransferAccessor baseAccessor = new ConsolePrintProductTransferAccessor();
            if (uploadResultToProduction)
                baseAccessor = new WebshopProductTransferAccessor(integration.WooClient);

            if (productDecorator != null)
            {
                baseAccessor = productDecorator.Decorate(integration, baseAccessor);
            }

            var erh = new WebshopErrorHandler(baseAccessor);

            hdlr = new TestErrorHandler(erh);


            var executor = new BiroToWooExecutorFactory(logger).Create(integration,
                                 testset,
                                 hdlr,
                                 new ConsoleDetectionAction(), // we do not want actual uploading.. we're just testing validation
                                 true,
                                 false // we don't want change detection here, we're only testing validation
                                 );

            await executor.Execute(integration, cancellationToken); // then verify if no errors as articles are all synchronized!
        }
    }

}
