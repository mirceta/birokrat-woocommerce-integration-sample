using infrastructure_pinger;
using lib_pinger;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tests.tools.fixture_setup;
using tests.tools.fixture_setup.synchronized;

namespace tests_bironext_pinger
{
    public class LocalMode_BironextPinger
    {
        public LocalMode_BironextPinger() { }

        private PingerForeverLoop loop;
        private infrastructure_pinger.BironextDeployment deployment;
        public infrastructure_pinger.BironextDeployment Deployment { get => deployment; }
        public int PingerDelaySeconds { get => 45; }

        private IMyLogger logger;
        public IMyLogger Logger { get => logger; }

        public async Task StartPinging(SynchronizedBironextResetter resetter, IMyLogger logger)
        {
            var pingerEventHandler = new LocalMode_BironextPingerEventHandler(resetter, logger);
            deployment = new infrastructure_pinger.BironextDeployment("BironextLocal",
                                                new Dictionary<string, string>() {
                                                        { "apiAddress", "http://localhost:19000/api/" },
                                                        { "apiKey", "dMepN2wPHm2pK/VAX8I1DyJ/PQsAQoByCsByMP+CTAA=" }
                                                });
            List<Deployment> deployments = new List<Deployment>() { deployment };
            int unsuccessfulPingThreshold = 3;
            IChainOfResponsibility pinger =
                new SyncDeploymentsStateComponents(
                        new PingerEventDetectorComponent(unsuccessfulPingThreshold, logger,
                            pingerEventHandler));
            loop = new PingerForeverLoop(pinger, deployments, PingerDelaySeconds);
            this.logger = logger;
            loop.Start();
        }

        public void Stop()
        {
            loop.Stop();
        }
    }
}
