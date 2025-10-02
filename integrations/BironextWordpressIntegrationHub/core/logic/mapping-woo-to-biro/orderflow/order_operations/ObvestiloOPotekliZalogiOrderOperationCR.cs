using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHubTests;
using core.logic.mapping_woo_to_biro.other;
using core.tools.wooops;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.customers.zgeneric {
    public class ObvestiloOPotekliZalogiOrderOperationCR : IOrderOperationCR {

        IApiClientV2 client;
        IOutApiClient wooclient;
        IOrderOperationCR next;
        public ObvestiloOPotekliZalogiOrderOperationCR(IApiClientV2 client, IOutApiClient wooclient, IOrderOperationCR next) {
            this.client = client;
            this.wooclient = wooclient;
            this.next = next;
        }

        public async Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data) {
            //ObvestiloOPotekliZalogi(order);

            if (next != null) {
                return await next.Next(order, data);
            }
            return data;
        }
        
        /*
        private void ObvestiloOPotekliZalogi(WoocommerceOrder order) {
            List<int> productIds = new List<int>();
            List<KeyValuePair<int, int>> productVariationIdS = new List<KeyValuePair<int, int>>();
            GetPostavkeIds(order, productIds, productVariationIdS);

            List<string> konecZaloga = new List<string>();
            foreach (int id in productIds) {
                string prod = wooclient.GetKita($"products/{id}");
                var obj = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(prod);
                string q = (string)obj["stock_quantity"];
                if (string.IsNullOrEmpty(q) || q == "0")
                    konecZaloga.Add(id + "");
            }

            foreach (var idpair in productVariationIdS) {
                string var = wooclient.GetKita($"products/{idpair.Key}/variations/{idpair.Value}");
                var obj = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(var);
                string q = (string)obj["stock_quantity"];
                if (string.IsNullOrEmpty(q) || q == "0")
                    konecZaloga.Add($"{idpair.Key}:{idpair.Value}");
            }

            // check zaloga of all stuff on the receipt!
            // use the product ids and variation ids and get the stock that way. If the stock is 0, then warn spica sport!!!
            // https://blog.elmah.io/how-to-send-emails-from-csharp-net-the-definitive-tutorial/
            if (konecZaloga.Count != 0) {
                new GmailSender("kristijanmircetatest@gmail.com", "kurackurac123")
                    .SendMail("prodaja@spica-sport.si", "Obvestilo o nakupu izdelka s preteklo zalogo", string.Join(", ", konecZaloga));
            }
        }*/

        private static void GetPostavkeIds(WoocommerceOrder order, List<int> productIds, List<KeyValuePair<int, int>> productVariationIdS) {
            foreach (var item in order.Items) {
                if (item.OriginProduct["variations"] != null && item.OriginProduct["variations"].Count > 0) {
                    dynamic variation = GWooOps.GetVariation(item);
                    int id = int.Parse(GWooOps.SerializeIntWooProperty(item.OriginProduct["id"]));
                    int variationid = int.Parse(GWooOps.SerializeIntWooProperty(variation["variation_id"]));
                    productVariationIdS.Add(new KeyValuePair<int, int>(id, variationid));
                } else {
                    int id = int.Parse(GWooOps.SerializeIntWooProperty(item.OriginProduct["id"]));
                    productIds.Add(id);
                }
            }
        }
    }
}
