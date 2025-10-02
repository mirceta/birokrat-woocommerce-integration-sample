using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure_pinger.chainofresponsibility
{
    class TerminatingPinger : IChainOfResponsibility
    {
        public Task Next(List<Deployment> deployments, Dictionary<string, object> additionalData) {
            return Task.CompletedTask;
        }

        public void Notification(string subject, string body)
        {
        }
    }
}
