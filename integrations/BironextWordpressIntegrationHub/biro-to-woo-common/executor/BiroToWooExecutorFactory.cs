using biro_to_woo.logic.change_trackers.exhaustive;
using biro_to_woo_common.error_handling.errors;
using biro_to_woo_common.executor.context_processor;
using biro_to_woo_common.executor.detection_actions;
using biro_to_woo_common.executor.validation;
using biro_to_woo_common.executor.validation.validation_stages.validators;
using biro_to_woo_common.executor.validation_stages;
using biro_to_woo_common.executor.validation_stages.change_trackers.exhaustive;
using BiroWooHub.logic.integration;
using core.error_handling.handlers;
using Microsoft.Extensions.Logging;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using tests_webshop.products;
using transfer_data.products;

namespace biro_to_woo_common.executor
{
    public class BiroToWooExecutorFactory
    {

        IMyLogger logger;
        public BiroToWooExecutorFactory(IMyLogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            this.logger = logger;
        }

        public BiroToWooExecutor SingleIterationTesting(IIntegration integration)
        {
            
            return new BiroToWooExecutor(logger,
                   new PersistedComparisonContextCreator(
                       new SimpleComparisonContextCreator(),
                       "context_cache1.json"
                   ),
                   new List<IBiroToOutValidationStage>()
                    {
                        new DatabaseAgreementComplianceVerifier().Get(integration),
                        new ExhaustiveArtikelChangeTrackerFactory(logger, 10).Create(integration)
                    },
                   new WebshopErrorHandler(new ConsolePrintProductTransferAccessor()),
                   new ConsoleDetectionAction()
            );
        }

        public BiroToWooExecutor ProductionLoop(IIntegration integration)
        {
            return new BiroToWooExecutor(logger,
                   new SimpleComparisonContextCreator(),
                   new List<IBiroToOutValidationStage>()
                    {
                        new DatabaseAgreementComplianceVerifier().Get(integration),
                        new ExhaustiveArtikelChangeTrackerFactory(logger, 10).Create(integration)
                    },
                   new WebshopErrorHandler(new WebshopProductTransferAccessor(integration.WooClient)),
                   new ArticleChangeUploader(integration, null)
            ); ;
        }

        public BiroToWooExecutor Create(IIntegration integration, 
            IComparisonContextCreator context, 
            IErrorHandler handler,
            IDetectionAction detectionAction,
            bool verify,
            bool detectChanges)
        {
            var stages = new List<IBiroToOutValidationStage>();
            if (verify)
                stages.Add(new DatabaseAgreementComplianceVerifier().Get(integration));
            if (detectChanges)
                stages.Add(new ExhaustiveArtikelChangeTrackerFactory(logger, 10).Create(integration));


            return new BiroToWooExecutor(logger,
                        context,
                        stages,
                        handler,
                        detectionAction);
        }
    }
}
