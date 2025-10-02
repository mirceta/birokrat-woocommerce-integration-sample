using BiroWoocommerceHubTests;
using System.Collections.Generic;

namespace biro_to_woo.logic.change_trackers.exhaustive
{
    public class ShopifyProductRetriever : IOutProductRetriever
    {

        public ShopifyProductRetriever() {}

        public List<Dictionary<string, object>> Get(IOutApiClient integ) {
            return integ.GetProducts().GetAwaiter().GetResult();
        }
    }
}
