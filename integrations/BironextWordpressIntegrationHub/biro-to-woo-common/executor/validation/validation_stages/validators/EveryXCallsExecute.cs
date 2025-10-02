using biro_to_woo_common.error_handling.reports;
using biro_to_woo_common.executor.validation_stages;
using core.logic.common_birokrat;
using core.structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.executor.validation.validation_stages.validators
{
    public class EveryXCallsExecute : IBiroToOutValidationStage
    {

        /*
            THIS CLASS ACTUALLY SUCKS: It will always send forth success reports for ALL items. This is wrong.
                                       You should actually remember during the validation iteration which items were
                                       bad and you should not let them forth....


            Warning: STATEFUL CLASS!
         THIS CLASS IS DANGEROUS: What if we are throwing away objects every time to avoid state,
               this component is by definition stateful. Is it a good idea?
         

         For usage where we want to call the underlying Validation stages only every few calls - 
         For example: We want to upload changes all the time, but we want to run
                      DatabaseAgreementComplianceVerifier_BTOStage only once every 10 times.
         */


        int numberOfCalls = 0;
        int currentCalls = 0;
        IBiroToOutValidationStage next;

        Func<Dictionary<string, object>, IOperationReport> createIgnoreReport;
        public EveryXCallsExecute(int numberOfCalls, BirokratField skuField, BirokratField attrField, IBiroToOutValidationStage next)
        {
            this.numberOfCalls = numberOfCalls;

            createIgnoreReport = (item) => new BiroToWooOperationReport(
                                skuField,
                                attrField,
                                item,
                                OperationOutcome.Ignore,
                                null
                            );
            currentCalls = 0;
            this.next = next;
        }

        public async Task<List<IOperationReport>> Execute(BiroOutComparisonContext ctx, CancellationToken token)
        {
            Func<bool> shouldPassthrough = () => numberOfCalls == 0 || currentCalls++ % numberOfCalls == 0;
            if (shouldPassthrough())
            {
                return await next.Execute(ctx, token);
            }
            else
            {
                return ctx.biroItems.Select(item => createIgnoreReport(item)).ToList();
            }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
