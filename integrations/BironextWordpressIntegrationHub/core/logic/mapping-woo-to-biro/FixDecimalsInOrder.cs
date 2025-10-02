using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHubTests;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace validator.logic {
    public class FixDecimalsInOrder : IOrderPostprocessor
    {

        IOutApiClient wooclient;

        public FixDecimalsInOrder(IOutApiClient wooclient) {
            this.wooclient = wooclient;
        }

        public WoocommerceOrder Postprocess(WoocommerceOrder order) {
            var tmp = wooclient.GetKita($"orders/{order.Data.Id}?dp=6").GetAwaiter().GetResult();
            var odr = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(tmp);

            try {
                string x = JsonConvert.SerializeObject(odr["line_items"]);
                var some = new { id = 0, subtotal = "", subtotal_tax = "", total = "", total_tax = "" };
                var items = JsonConvert.DeserializeAnonymousType(x, new[] { some }) ;

                foreach (var item in order.Items) {
                    foreach (var item2 in items) {
                        if (item.Id == item2.id) {
                            item.Total = item2.total;
                            item.TotalTax = item2.total_tax;
                            item.Subtotal = item2.subtotal;
                            item.SubtotalTax = item2.subtotal_tax;
                            break;
                        }
                    }
                }
            } catch (Exception ex) {

            }
            return order;
        }
    }
}