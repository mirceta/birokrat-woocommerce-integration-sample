using biro_to_woo_common.error_handling.errors;
using biro_to_woo_common.executor;
using biro_to_woo_common.executor.context_processor;
using biro_to_woo_common.executor.detection_actions;
using biro_to_woo_common.executor.validation_stages.change_trackers.exhaustive;
using biro_to_woo_common.executor.validation_stages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using tests_webshop.products;
using core.logic.mapping_biro_to_woo;
using BiroWooHub.logic.integration;
using common_birowoo;
using System.Linq;
using biro_to_woo_common.executor.validation.validation_stages.validators;
using System.Globalization;
using si.birokrat.next.common.logging;
using System.Threading;
using tests.composition.common;
using System.Diagnostics;
using transfer_data.products;

namespace tests.composition.final_composers.production
{
    public class ProductProduction
    {

        bool debug;
        public ProductProduction(bool debug = false)
        {
            this.debug = debug;
        }

        public async Task Execute(
            SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator,
            IMyLogger logger,
            IIntegration integration)
        {
            BiroToWooExecutor executor = composeExecutor_With_WebshopAccessor(productDecorator, logger, integration);
            await singleIteration(logger, integration, executor);
        }

        private async Task singleIteration(IMyLogger logger, IIntegration integration, BiroToWooExecutor executor)
        {
            var start = new RandomDelayedStart(integration.Name, integration.BiroClient, logger);
            try
            {


                if (!debug)
                {
                    await start.Prod(async () =>
                    {
                        //await DeleteOldProductTransfers(integration);
                        await executor.Execute(integration, new CancellationToken());
                    });
                }
                else {
                    await start.Test(async () =>
                    {
                        //await DeleteOldProductTransfers(integration);
                        await executor.Execute(integration, new CancellationToken());
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation("Exception caught in root. Now starting new iteration");
            }
        }

        private static BiroToWooExecutor composeExecutor_With_WebshopAccessor(SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator, IMyLogger logger, IIntegration integration)
        {
            // this needs to go because right now, ProductTransferAccessor assumes that shit is on the webpage, but later
            // we will have one that keeps data locally.

            IProductTransferAccessor baseAccessor = new WebshopProductTransferAccessor(integration.WooClient);
            if (productDecorator != null)
            {
                baseAccessor = productDecorator.Decorate(integration, baseAccessor);
            }

            var errorHandler = new WebshopErrorHandler(baseAccessor);

            var stages = new List<IBiroToOutValidationStage>();


            // VALIDATION PHASE SHOULD NEVER BE EXECUTED AS A PART OF MAIN PRODUCTION LOOP
            // IT IS VERY EXPENSIVE AND SHOULD BE USED AS A SEPARATE TASK DEPLOYED FROM THE
            // WEB APPLICATION!
            //if (integration.Options["birotowoo_dont_include_validator"] == "true") { }
            //else
            //{
            //    stages.Add(new DatabaseAgreementComplianceVerifier().Get(integration));
            //}
            stages.Add(new ExhaustiveArtikelChangeTrackerFactory(logger, 10).Create(integration));

            var executor = new BiroToWooExecutor(logger: logger,
               cmp: new SimpleComparisonContextCreator(),
               stages: stages,
               errorHandler: errorHandler,
               detectionAction: new ArticleChangeUploader(integration,
                                        (x) => new ReportingBiroToWoo(errorHandler, integration.BiroToWoo)));
            return executor;
        }

        private static async Task DeleteOldProductTransfers(IIntegration integration)
        {
            /*
             This should be done because 
             */

            var ws = new WebshopProductTransferAccessor(integration.WooClient);
            var neki = await ws.List();

            neki = neki
                    .Where(x => DateTime.Now.Subtract( // take only those that are older than 1 day
                        DateTime.ParseExact(x.last_event_datetime, "yyyy-MM-ddHH:mm:ss", CultureInfo.InvariantCulture)).TotalHours > 24)
                    .ToList();

            neki = neki
                    .Take(Math.Min(100, neki.Count)) // guard against taking 1000 of them
                    .ToList();

            foreach (var x in neki)
            {

                await new WebshopProductTransferAccessor(integration.WooClient).DeleteProductTransfer(x.product_id);
            }
        }

        // Preserving the handleBiroToWooCase and handleWooToBiroCase methods which seem to be defined elsewhere
        // Note: These methods should be defined in the TestsFactoryCreator class or passed as dependencies
    }
}
