using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests.tests.estrada
{
    // We cannot reuse OrderTransferProcessorStage because it's used for other things!
    public interface IOrderActStage {
        Task<Dictionary<string, object>> Act(IIntegration integration, WoocommerceOrder order);
    }
}
