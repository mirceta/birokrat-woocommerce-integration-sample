using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWooHub.logic.integration
{
    public interface IWooToBiro
    {


        Task OnArticleAdded(WoocommerceProduct product);
        Task OnArticleChanged(Dictionary<string, WoocommerceProduct> product);
        Task OnArticleAddedRaw(string product_id, string variation_id);
        Task OnArticleChangedRaw(string product_id, string variation_id);

        Task<Dictionary<string, object>> OnOrderStatusChanged(string order);

        Task<object> OnAttachmentRequest(string order);
    }
}
