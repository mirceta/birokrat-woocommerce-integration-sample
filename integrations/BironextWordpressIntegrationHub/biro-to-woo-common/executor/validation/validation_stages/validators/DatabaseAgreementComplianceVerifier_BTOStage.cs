using biro_to_woo_common.error_handling.reports;
using biro_to_woo_common.executor.validation_stages.validators.validation_operations;
using birowoo_exceptions;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.structs;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.executor.validation_stages.validators
{
    public class DatabaseAgreementComplianceVerifier_BTOStage : IBiroToOutValidationStage
    {

        IIntegration integration;
        List<IProductTransferVerifyOperation> flow;
        public DatabaseAgreementComplianceVerifier_BTOStage(IIntegration integration, List<IProductTransferVerifyOperation> flow)
        {
            this.integration = integration;
            this.flow = flow;

        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public async Task<List<IOperationReport>> Execute(BiroOutComparisonContext ctx, CancellationToken token)
        {
            var result = Validate(ctx);
            return result;
        }

        private List<IOperationReport> Validate(BiroOutComparisonContext cmp)
        {
            string skuAttr = BirokratNameOfFieldInFunctionality.SifrantArtiklov(integration.BiroToWoo.SkuBirokratField);


            List<IOperationReport> reports = new List<IOperationReport>();
            foreach (var item in cmp.biroItems)
            {
                //Console.WriteLine(DateTime.Now);
                var report = CreateReport(cmp, skuAttr, item);
                reports.Add(report);

            }
            return reports;
        }

        private IOperationReport CreateReport(BiroOutComparisonContext cmp, string skuAttr, Dictionary<string, object> item)
        {
            Func<OperationOutcome, Exception, BiroToWooOperationReport> createOperation = (outcome, ex) =>
            {
                return new BiroToWooOperationReport(
                                integration.BiroToWoo.SkuBirokratField,
                                integration.BiroToWoo.VariableProductBirokratField,
                                item,
                                outcome,
                                ex
                            );
            };

            foreach (var stage in flow)
            {
                try
                {
                    stage.Verify((string)item[skuAttr], cmp);
                }
                catch (CannotValidateNonSyncedProductException ex)
                {
                    // in this stage non syncer products should be successful - because they are not wrongly synced yet, they simply are not synced.
                    // we want these to go into the next stage!
                    return createOperation(OperationOutcome.Success, ex);
                }
                catch (IntegrationProcessingException ex)
                {
                    return createOperation(OperationOutcome.Error, ex);
                }
                catch (Exception ex)
                {
                    return createOperation(OperationOutcome.Error, ex);
                }
            }
            return createOperation(OperationOutcome.Success, null);
        }
    }
}
