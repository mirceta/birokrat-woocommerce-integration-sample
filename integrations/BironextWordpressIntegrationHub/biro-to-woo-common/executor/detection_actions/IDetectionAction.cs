using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.executor.detection_actions
{
    public interface IDetectionAction
    {
        Task NotifyChanges(List<string> successfulItemSifras, CancellationToken token);
    }
}
