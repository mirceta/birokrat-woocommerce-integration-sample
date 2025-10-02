using BiroWoocommerceHubTests;
using System.Collections.Generic;

namespace biro_to_woo.logic.change_trackers.exhaustive
{
    public interface IOutProductRetriever {
        List<Dictionary<string, object>> Get(IOutApiClient integ);
    }
}
