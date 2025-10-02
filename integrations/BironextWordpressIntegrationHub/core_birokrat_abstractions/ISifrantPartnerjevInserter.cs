using BironextWordpressIntegrationHub.structs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{
    public interface ISifrantPartnerjevInserter {
        Task<string> EnforceWoocommerceBillingPartnerCreated(WoocommerceOrder order, Dictionary<string, string> additionalInfo);
    }
}
