using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.logic;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.logic.mapping_woo_to_biro;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.customers.poledancerka.mappers
{
    public class DancerkaComplexProductMapper : IWooToBiroProductMapper
    {

        IApiClientV2 client;
        int variationAttributeCountThreshold;
        BirokratPostavkaUtils utils;
        bool putSkuInComment;

        public DancerkaComplexProductMapper(BirokratPostavkaUtils utils, IApiClientV2 client, bool putSkuInComment, int variationAttributeCountThreshold = 2)
        {
            this.client = client;
            this.variationAttributeCountThreshold = variationAttributeCountThreshold;
            this.utils = utils;
            this.putSkuInComment = putSkuInComment;
        }

        public string GetOrAddProductAndReturnSifra()
        {
            throw new NotImplementedException();
        }

        public bool IsThisTypeOfProduct(dynamic x)
        {
            return GWooOps.OriginProductAttributeCount(x) >= variationAttributeCountThreshold;
        }

        public Task MapWooProductToBirokrat()
        {
            throw new NotImplementedException();
        }

        public Task MapWooProductToBirokrat(Dictionary<string, object> product)
        {
            throw new NotImplementedException();
        }

        public async Task<BirokratPostavka> ProductItemToBirokratPostavka(WoocommerceOrderItem x, bool verifyAndCreate) {
            
            var postavka = utils.Get(x, x.BirokratSifra);
            postavka.BirokratSifra = "";

            string[] some = ((string)x.OriginProduct["sku"]).Split("/");
            List<KeyValuePair<string, string>> pairs = GWooOps.ForVariation_WithId_GetAttributeKeyValuePairs(x.OriginProduct, x.VariationId);
            var res = some.Zip(pairs, (x, y) => x + "/" + y.Value.ToUpper());
            string sku = string.Join("-", res);
            string hash = Tools.GetHashCode(sku);

            postavka.BirokratSifra = hash;

            if (putSkuInComment) {
                postavka.Comment = $"SKU: {((string)x.OriginProduct["sku"])}";
            }
            return postavka;
        }
    }
}
