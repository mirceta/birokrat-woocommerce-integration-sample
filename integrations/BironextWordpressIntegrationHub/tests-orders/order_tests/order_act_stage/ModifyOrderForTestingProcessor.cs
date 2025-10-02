using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using tests.tools;

namespace tests.tests.estrada
{
    public class ModifyOrderForTestingProcessor : IOrderActStage {
        int sessionId;
        IOrderActStage next;
        public ModifyOrderForTestingProcessor(int sessionId, IOrderActStage next) {
            this.sessionId = sessionId;
            this.next = next;
        }
        public async Task<Dictionary<string, object>> Act(IIntegration integration, WoocommerceOrder order) {
            order = TestUtils.ModifyOrderForTesting(JsonConvert.SerializeObject(order), sessionId);
            return await next.Act(integration, order); 
        }
    }
}
