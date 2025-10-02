using infrastructure_pinger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace lib_pinger
{
    public class PingerForeverLoop
    {
        List<Deployment> deployments;
        int delayBetweenIterations;
        IChainOfResponsibility pinger;
        internal bool IsActive;

        public PingerForeverLoop(IChainOfResponsibility pinger, List<Deployment> deployments, int delayBetweenIterations)
        {
            this.pinger = pinger;
            this.deployments = deployments;
            this.delayBetweenIterations = delayBetweenIterations;
            IsActive = true;
        }

        public async Task Start() {
            await pinger.Next(deployments, null);
            pinger = new PingerDelay(delayBetweenIterations,
                     pinger);
            while (IsActive)
            {
                await pinger.Next(deployments, null);
            }
        }

        public void Stop()
        {
            IsActive = false;
        }
    }
}
