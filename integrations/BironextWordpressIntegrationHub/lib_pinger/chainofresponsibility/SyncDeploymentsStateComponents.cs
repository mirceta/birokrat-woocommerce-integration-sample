using infrastructure_pinger.chainofresponsibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace infrastructure_pinger
{

    public class SyncDeploymentsStateComponents : IChainOfResponsibility
    {

        List<Deployment> deployments;
        IChainOfResponsibility next;

        public SyncDeploymentsStateComponents(IChainOfResponsibility next)
        {
            this.next = next;
        }

        public async Task Next(List<Deployment> deployments, Dictionary<string, object> additionalData) {
            Console.WriteLine("PINGER MAIN");
            this.deployments = deployments;

            try {
                await PingingHelper.Pingy(deployments);
            } catch (Exception ex) {
                Console.WriteLine("Fatal error: this should never happen in this application.");
            }

            await next.Next(deployments, additionalData);
        }
    }
}
