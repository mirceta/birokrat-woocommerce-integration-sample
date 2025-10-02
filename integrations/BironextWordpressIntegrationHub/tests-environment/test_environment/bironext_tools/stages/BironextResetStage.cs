using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using tests.tools.fixture_setup;
using tests.tools.fixture_setup.synchronized;

namespace tests.tests.estrada
{
    public class BironextResetStage : ISetupStage
    {
        WaitUntilBironextOnlineForSure x;

        public BironextResetStage(WaitUntilBironextOnlineForSure x)
        {
            this.x = x;
        }

        public async Task Work()
        {
            await x.Wait();
        }
    }
}
