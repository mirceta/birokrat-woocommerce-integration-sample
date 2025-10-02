using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests.tests.estrada
{
    public class SetOrderStatusProcessor : IOrderActStage {

        string status;
        IOrderActStage next;
        public SetOrderStatusProcessor(string status, IOrderActStage next) {
            this.status = status;
            this.next = next;
        }

        public async Task<Dictionary<string, object>> Act(IIntegration integration, WoocommerceOrder order) {
            order.Data.Status = status;
            return await next.Act(integration, order);
        }
    }
}
