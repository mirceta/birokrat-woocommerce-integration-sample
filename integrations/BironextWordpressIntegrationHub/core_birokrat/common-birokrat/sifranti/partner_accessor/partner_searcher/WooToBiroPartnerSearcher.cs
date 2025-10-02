using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using core.logic.common_woo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{

    public interface IWooToBiroPartnerSearcher {
        Task<Dictionary<string, object>> MatchWooToBiroUser(WoocommerceOrder order, Dictionary<string, string> additionalInfo);
    }
}
