using System.Collections.Generic;
using System.Threading.Tasks;

namespace infrastructure_pinger.chainofresponsibility.eventhandlers
{
    public interface IPingerEventHandler
    {
        Task OnServiceRestore(List<Deployment> restored);
        Task onServiceFailure(List<Deployment> failed);
        Task onLongHeartbeat(List<Deployment> deployments);
        Task onWarning(List<Deployment> potentiallyFailed);
    }
}
