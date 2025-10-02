using System.Threading.Tasks;

namespace tests.tests.estrada
{
    public class WaitUntilBironextOnlineForSure {

        infrastructure_pinger.BironextDeployment deployment;
        int pingerIterationLengthSeconds;
        public WaitUntilBironextOnlineForSure(infrastructure_pinger.BironextDeployment deployment, int pingerIterationLengthSeconds)
        {
            this.deployment = deployment;
            this.pingerIterationLengthSeconds = pingerIterationLengthSeconds;
        }
        public async Task Wait()
        {

            // first delay should be long enough for pinger to also confirm that the service is down.
            await Task.Delay(pingerIterationLengthSeconds * 1000);

            // other delays are waiting for the pinger to resolve the problem
            do
            {
                await Task.Delay(5000);
            } while (deployment != null && deployment.UnsuccessfulPingsInARow > 0);
        }

    }
}
