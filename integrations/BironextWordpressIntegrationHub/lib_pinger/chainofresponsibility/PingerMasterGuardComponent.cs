using infrastructure_pinger.chainofresponsibility;
using infrastructure_pinger.deployment;
using si.birokrat.next.common.networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure_pinger
{
    public class PingerMasterGuardComponent : IChainOfResponsibility
    {
        IChainOfResponsibility next;
        int unsuccessfulPingsThreshold;
        public PingerMasterGuardComponent(int unsuccessfulPingsThreshold, IChainOfResponsibility next)
        {
            this.next = next;
            this.unsuccessfulPingsThreshold = unsuccessfulPingsThreshold;
        }

        public async Task Next(List<Deployment> deployments, Dictionary<string, object> additionalData)
        {
            Console.WriteLine("PINGER MASTER GUARD COMPONENT");
            await PingingHelper.Pingy(deployments.Where(x => x.Name.Contains("Pinger")).ToList());
            if (AmITheCurrentMasterPinger(deployments)) {
                Console.WriteLine("I AM MASTER");
                await next.Next(deployments, additionalData);
            }
        }

        bool AmITheCurrentMasterPinger(List<Deployment> deployments)
        {
            string myip = NetworkingUtils.GetLocalIPAddress();

            var master_pinger = deployments
                .Where(x => x is PingerDeployment && x.UnsuccessfulPingsInARow < unsuccessfulPingsThreshold) // is alive?
                .Select(x => x.AdditionalInfo["ipaddress"])
                .OrderBy(x => x
                ).First();

            Console.WriteLine($"Current master: {master_pinger}");
            return myip == master_pinger;
        }
    }
}
