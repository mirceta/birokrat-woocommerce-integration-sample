using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace infrastructure_pinger
{
    public interface IChainOfResponsibility {
        Task Next(List<Deployment> deployments, Dictionary<string, object> additionalData);
    }

    public class NullChainOfResponsibilityHandler : IChainOfResponsibility
    {
        public Task Next(List<Deployment> deployments, Dictionary<string, object> additionalData)
        {
            // Return a completed task with no action taken
            return Task.CompletedTask;
        }
    }

}
