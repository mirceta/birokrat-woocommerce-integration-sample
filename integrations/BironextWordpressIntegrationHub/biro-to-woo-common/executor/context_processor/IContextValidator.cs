using biro_to_woo_common.error_handling.reports;
using core.structs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.executor.context_processor
{
    public interface IContextValidator
    {
        Task Validate(BiroOutComparisonContext context, CancellationToken token);
        List<IOperationReport> GetFailedItems();
        List<string> GetSuccessfulItemSifras();
        List<string> GetNeutralItems();
    }
}
