using biro_to_woo_common.error_handling.errors;
using biro_to_woo_common.error_handling.reports;
using biro_to_woo_common.executor.context_processor;
using biro_to_woo_common.executor.detection_actions;
using biro_to_woo_common.executor.validation_stages;
using BiroWooHub.logic.integration;
using core.structs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.executor
{

    public class BiroToWooExecutor
    {

        IMyLogger logger;
        List<IBiroToOutValidationStage> stages;
        IComparisonContextCreator cmp;
        IErrorHandler errorHandler;
        IDetectionAction action;

        public BiroToWooExecutor(IMyLogger logger,
            IComparisonContextCreator cmp,
            List<IBiroToOutValidationStage> stages,
            IErrorHandler errorHandler,
            IDetectionAction detectionAction)
        {
            this.logger = logger;
            this.stages = stages;
            this.cmp = cmp;
            this.errorHandler = errorHandler;
            action = detectionAction;
        }

        public async Task Execute(IIntegration integration, CancellationToken token)
        {

            logger.LogInformation($"Starting new biro-to-woo executor iteration");
            lastRunContext = await cmp.Create(integration, token);
            
            var contextValidator = new ContextValidator(stages);
            await contextValidator.Validate(lastRunContext, token);

            var errors = contextValidator.GetFailedItems();
            logger.LogInformation($"Uploading {errors.Count} errors");

            errors.ForEach(x => errorHandler.HandleError(x));

            if (errors.Count == 0)
                logger.LogInformation($"No configuration errors. All articles are correctly configured.");

            var skoe = contextValidator.GetNeutralItems();

            var successfulSifras = contextValidator.GetSuccessfulItemSifras();
            successfulSifras = successfulSifras.Distinct().ToList();
            logger.LogInformation($"Uploading {successfulSifras.Count} success items");
            await action.NotifyChanges(successfulSifras, token);
        }

        BiroOutComparisonContext lastRunContext;
        public BiroOutComparisonContext LastRunContext() {
            return lastRunContext;
        }
    }

    class ValidationStagesResult
    {
        public List<IOperationReport> reportsOnFailures;
        public List<Dictionary<string, object>> successfulItems;
    }
}
