using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using System.Collections.Generic;
using System.IO;
using tests.tests.estrada;

namespace tests.tools
{
    public class OrderProgressKeeperFactory : IProgressKeeperFactory {

        bool reset;
        AlreadyProcessedFilter filter;
        string specificIntegrationDataPath;
        public OrderProgressKeeperFactory(string specificIntegrationDataPath, bool reset, AlreadyProcessedFilter filter) {
            this.reset = reset;
            this.filter = filter;
            this.specificIntegrationDataPath = specificIntegrationDataPath;
        }


        public IProgressKeeper Create(IIntegration integration) {
            var tmp = new FileProgressKeeper<WoocommerceOrder>(
                Path.Combine(integration.Datafolder, specificIntegrationDataPath, "alreadyprocessed.json"),
                new OrderSigner(),
                filter);
            if (reset)
                tmp.Restart();
            return tmp;
        }

    }

    public interface ISigner<T>
    {
        public string GetSignature(T some);
    }

    public class OrderSigner : ISigner<WoocommerceOrder>
    {
        public string GetSignature(WoocommerceOrder some) {
            return TestUtils.OrderNumHash(some) + "" + some.Data.Status;
        }
    }
}
