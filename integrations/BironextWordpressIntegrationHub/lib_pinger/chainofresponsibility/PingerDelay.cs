using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace infrastructure_pinger
{
    public class PingerDelay : IChainOfResponsibility
    {
        IChainOfResponsibility next;
        int minEpochLengthSeconds;
        public PingerDelay(int minEpochLengthSeconds, IChainOfResponsibility next) {
            this.next = next;
            this.minEpochLengthSeconds = minEpochLengthSeconds;
        }

        public async Task Next(List<Deployment> deployments, Dictionary<string, object> additionalData) {
            DateTime start = DateTime.Now;
            await next.Next(deployments, additionalData);
            await Task.Delay(Math.Max(minEpochLengthSeconds * 1000 - (int)DateTime.Now.Subtract(start).TotalMilliseconds, 0));
        }

    }
}
