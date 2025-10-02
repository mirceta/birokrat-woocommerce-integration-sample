using BiroWoocommerceHub.flows;
using BiroWoocommerceHubTests;
using core.customers.poledancerka.mappers;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.product_ops {

    public class PoledancerkaWooToBiroProductInserter : IWooToBiroProductInserter {

        IWooToBiroProductMapper simpleMapper;
        IWooToBiroProductMapper variableMapper;
        IWooToBiroProductMapper complexMapper;
        IOutApiClient wooclient;

        public PoledancerkaWooToBiroProductInserter(
            IOutApiClient wooclient,
            IWooToBiroProductMapper simpleMapper,
            IWooToBiroProductMapper variableMapper,
            IWooToBiroProductMapper complexMapper) {
            this.wooclient = wooclient;
            this.simpleMapper = simpleMapper;
            this.variableMapper = variableMapper;
            this.complexMapper = complexMapper;

        }

        public async Task OnArticleAddedRaw(string product_id, string variation_id) {


            /*
            string some = wooclient.Get($"products/{product_id}");
            var dic = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(some);
            */
            /*var dic = wooclient.GetProduct(product_id).GetAwaiter().GetResult();



            var attrs = dic["attributes"];
            int attrlen = Tools.RetardedDynamicLength(attrs);

            string type = (string)dic["type"];

            if (attrlen > 1) {
                // complex product - we do not add it!
            } else if (type == "variable") { // variable product
            } else if (type == "variation") {
                await variableMapper.MapWooProductToBirokrat(dic);
            } else if (type == "simple") {
                // add simple product
                await simpleMapper.MapWooProductToBirokrat(dic);
            }
            */
        }

        public async Task OnArticleChangedRaw(string product_id, string variation_id) {
            // not supported
        }

    }
}
