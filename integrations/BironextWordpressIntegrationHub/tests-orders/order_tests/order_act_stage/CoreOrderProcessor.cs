using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests.tests.estrada
{
    public class CoreOrderProcessor : IOrderActStage {
        
        public CoreOrderProcessor() {
            
        }

        public async Task<Dictionary<string, object>> Act(IIntegration integration, WoocommerceOrder order) {
            return await integration.WooToBiro.OnOrderStatusChanged(JsonConvert.SerializeObject(order));
        }
    }
}
