using biro_to_woo.logic.change_trackers.exhaustive;
using BiroWoocommerceHubTests;
using core.tools.wooops;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace webshop_client_woocommerce.product_retriever
{
    public class WooProductRetriever : IOutProductRetriever
    {

        int parallelTaskCount;
        ILogger logger;

        public WooProductRetriever(int parallelTaskCount, ILogger logger)
        {
            this.parallelTaskCount = parallelTaskCount;
            this.logger = logger;
        }

        public List<Dictionary<string, object>> Get(IOutApiClient integ)
        {
            WooSimpleProductRetriever wooProductRetriever = new WooSimpleProductRetriever(
                new ProductRetrieverAsyncOperationFactory(logger),
                parallelTaskCount);
            WooVariationRetriever wooVariationRetriever = new WooVariationRetriever(
                new VariationRetrieverAsyncOperationFactory(logger),
                parallelTaskCount);

            var products = wooProductRetriever.Get(integ);

            // WE SAW THAT THERE MAY BE DUPLICATIONS BECAUSE NEIGHBORING PAGES OVERLAP ONE ANOTHER!!!!
            products = products
                .GroupBy(p => GWooOps.SerializeIntWooProperty(p["id"]))
                .Select(g => g.First())
                .ToList();

            List<string> filtered = new List<string>();
            Dictionary<string, string> skuToStatus = new Dictionary<string, string>();
            foreach (var prod in products)
            {
                var variations = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(prod["variations"]));
                if (variations.Count > 0) {
                    filtered.Add(GWooOps.SerializeIntWooProperty(prod["id"]));
                    skuToStatus[GWooOps.SerializeIntWooProperty(prod["id"])] = prod["status"] as string;
                }

            }
            if (filtered.Count > 0)
            {
                var vars = wooVariationRetriever.Get(integ, filtered);

                foreach (var vari in vars) {
                    vari["status"] = skuToStatus[vari["parent_id"] as string];
                }
                
                products.AddRange(vars);
            }
            return products;
        }
    }
}
