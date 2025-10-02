using infrastructure_pinger.chainofresponsibility.eventhandlers;
using infrastructure_pinger.deployment;
using si.birokrat.next.common.logging;
using si.birokrat.next.common.networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure_pinger
{


    public class PingerEventDetectorComponent : IChainOfResponsibility
    {
        List<Deployment> deployments;
        int unsuccessfulPingsThreshold;
        IPingerEventHandler eventHandler;
        IMyLogger logger;
        public PingerEventDetectorComponent(int unsuccessfulPingsThreshold,
                                          IMyLogger logger,
                                          IPingerEventHandler eventHandler) {
            this.unsuccessfulPingsThreshold = unsuccessfulPingsThreshold;
            this.eventHandler = eventHandler;
            this.logger = logger;
        }

        public async Task Next(List<Deployment> deployments, Dictionary<string, object> additionalData)
        {
            this.deployments = deployments;
            await PositiveFrontNotification();
            await NegativeFrontNotification();
            bool time_for_notification = DateTime.Now.Hour % 6 == 0 && DateTime.Now.Minute < 2;
            if (time_for_notification) {
                await LongHeartbeatNotification();
            }
        }

        #region [auxiliary]
        private async Task PositiveFrontNotification() {
            var successes = deployments.Where(x => FailureNotDetected(x) && x.AwaitingResolution).ToList();
            if (successes.Count > 0) {
                await eventHandler.OnServiceRestore(successes);
            }
            successes.ForEach(x => x.AwaitingResolution = false);
        }

        private async Task LongHeartbeatNotification() {
            await eventHandler.onLongHeartbeat(deployments);
        }

        private async Task NegativeFrontNotification() {
            var fails = deployments.Where(x => !FailureNotDetected(x) && !x.AwaitingResolution).ToList();
            deployments.ForEach(x => logger.LogWarning($"{x.Name} fails:{x.UnsuccessfulPingsInARow}"));
            if (fails.Count > 0) {
                await eventHandler.onServiceFailure(fails);
            }
            fails.ForEach(x => x.AwaitingResolution = true);
        }

        private bool FailureNotDetected(Deployment dep) {
            return dep.UnsuccessfulPingsInARow < unsuccessfulPingsThreshold;
        }
        #endregion
    }
}
