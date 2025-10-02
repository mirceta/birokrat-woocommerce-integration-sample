using infrastructure_pinger.chainofresponsibility.eventhandlers;
using infrastructure_pinger;
using System.Collections.Generic;
using System.Threading.Tasks;
using tests.tools.fixture_setup.synchronized;
using si.birokrat.next.common.logging;
using System;

namespace tests_bironext_pinger
{
    public class LocalMode_BironextPingerEventHandler : IPingerEventHandler
    {
        SynchronizedBironextResetter resetter;
        IMyLogger logger;
        public LocalMode_BironextPingerEventHandler(SynchronizedBironextResetter resetter, IMyLogger logger)
        {
            this.resetter = resetter;
            this.logger = logger;
        }

        public async Task OnServiceRestore(List<Deployment> restored)
        {
            // Intentionally left blank
        }

        public async Task onServiceFailure(List<Deployment> failed)
        {
            try
            {
                logger.LogInformation("Service failure detected. Attempting to restart...");
                await resetter.Reset();
                logger.LogInformation("Restart successful!");
            }
            catch (Exception ex) {
                logger.LogError("The restart was unsuccessful " + ex.Message + ex.StackTrace.ToString());
            }
        }

        public async Task onLongHeartbeat(List<Deployment> deployments)
        {
            // Intentionally left blank
        }

        public async Task onWarning(List<Deployment> potentiallyFailed)
        {
            // Intentionally left blank
        }
    }
}
