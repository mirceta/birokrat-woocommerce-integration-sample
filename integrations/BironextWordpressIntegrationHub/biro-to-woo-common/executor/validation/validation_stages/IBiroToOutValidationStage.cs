using biro_to_woo_common.error_handling.reports;
using core.structs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.executor.validation_stages
{
    public interface IBiroToOutValidationStage
    {
        Task<List<IOperationReport>> Execute(BiroOutComparisonContext ctx, CancellationToken token);
        void Clear();
    }
}
