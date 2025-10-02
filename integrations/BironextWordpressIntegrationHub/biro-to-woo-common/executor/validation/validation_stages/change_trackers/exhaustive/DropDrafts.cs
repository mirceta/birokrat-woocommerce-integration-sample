using biro_to_woo_common.error_handling.reports;
using birowoo_exceptions;
using core.structs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.executor.validation_stages.change_trackers.exhaustive
{
    public class DropDrafts : IBiroToOutValidationStage
    {
        IBiroToOutValidationStage next;
        public DropDrafts(IBiroToOutValidationStage next) {
            this.next = next;
        }
        public void Clear()
        {
            next.Clear();
        }

        public async Task<List<IOperationReport>> Execute(BiroOutComparisonContext ctx, CancellationToken token)
        {
            if (ctx == null || ctx.outItems == null || ctx.biroItems == null)
                throw new IntegrationProcessingException("The comparison context is empty");

            var filtered_products = ctx.outItems.Where(x => (string)x["status"] != "draft").ToList();
            var new_ctx = new BiroOutComparisonContext();
            new_ctx.outItems = filtered_products;
            new_ctx.biroItems = ctx.biroItems;
            return await next.Execute(new_ctx, token);
        }
    }
}
