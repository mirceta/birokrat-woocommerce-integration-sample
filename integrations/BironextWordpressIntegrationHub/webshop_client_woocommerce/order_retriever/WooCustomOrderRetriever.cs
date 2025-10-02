
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHubTests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace validator.logic.order_transfer.accessor
{
    public class WooCustomOrderRetriever : IOrderRetriever
    {

        IOutApiClient wooclient;

        public WooCustomOrderRetriever(IOutApiClient wooclient) {
            if (wooclient == null)
                throw new ArgumentNullException("wooclient");
            this.wooclient = wooclient;
        }

        public async Task<string> GetOrder(string id) {
            string some = await wooclient.MyGetOrder(id);
            string chome = await wooclient.Get($"orders/{id}");
            WoocommerceOrder x = AddLineItemSkusToOrder(some, chome);
            return JsonConvert.SerializeObject(x);
        }

        private static WoocommerceOrder AddLineItemSkusToOrder(string some, string chome) {
            var x = JsonConvert.DeserializeObject<WoocommerceOrder>(some);
            var y = JsonConvert.DeserializeObject<Dictionary<string, object>>(chome);


            var ch = new[] { new { sku = "" } };
            var res = JsonConvert.SerializeObject(y["line_items"]);

            var kurac = JsonConvert.DeserializeAnonymousType(res, ch);
            for (int i = 0; i < x.Items.Count; i++) {
                string sku = kurac[i].sku;
                x.Items[i].Sku = sku;
            }

            return x;
        }
    }
}
