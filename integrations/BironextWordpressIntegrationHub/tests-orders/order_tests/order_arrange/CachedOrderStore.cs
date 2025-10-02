using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHubTests;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using tests.tests.estrada;
using validator.logic;
using validator.logic.order_transfer.accessor;

namespace tests_fixture
{
    public class CachedOrderStore : IOrderStore
    {

        FolderOrderStore next;
        IOutApiClient wooclient;
        IOrderPostprocessor orderPostprocessor;

        int maxDaysOld;
        int maxCount;

        public CachedOrderStore(
            IOutApiClient wooclient,
            IOrderPostprocessor orderPostprocessor,
            int maxdaysold = 1800,
            int maxCount = 500000,
            FolderOrderStore next = null) {
            if (next == null)
                throw new ArgumentNullException("next");
            if (wooclient == null)
                throw new ArgumentNullException("wooclient");
            if (orderPostprocessor == null)
                throw new ArgumentNullException("orderPostprocessor");
            this.next = next;
            this.wooclient = wooclient;
            this.orderPostprocessor = orderPostprocessor;
            this.maxDaysOld = maxdaysold;
            this.maxCount = maxCount;
        }

        public async Task<List<string>> GetOrders() {
            List<string> currentOrderIds = await GetOrdersFromFolder();
            List<string> webshopIds = GetOrdersFromWebshop();
            var missingOrders = webshopIds.Except(currentOrderIds).ToList();
            await CacheNewOrders(missingOrders);

            return await next.GetOrders();
        }

        private async Task CacheNewOrders(List<string> missingOrders) {
            foreach (var missing in missingOrders) {
                string a = await wooclient.MyGetOrder(missing);
                var order = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<WoocommerceOrder>(a);
                if (order == null || order.Items == null) {
                    Console.WriteLine($"Unable to deserialize order {missing}");
                    continue;
                }
                order = orderPostprocessor.Postprocess(order);
                next.SaveOrder(order);
            }
        }

        private List<string> GetOrdersFromWebshop() {
            
            List<string> webshopIds = new WooOrderRetrieverHelper(maxDaysOld, maxCount, wooclient).GetOrderIds();
            return webshopIds;
        }

        private async Task<List<string>> GetOrdersFromFolder() {
            return (await next.GetOrders())
                            .Select(x => new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<WoocommerceOrder>(x))
                            .Select(x => x.Data.Id + "").ToList();
        }
            
    }
}
