using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.logic;
using BiroWooHub.logic.integration;
using core.customers.zgeneric;
using core.logic.mapping_woo_to_biro.product_ops;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace allintegrations.customers {
    public class OrderFlowProductInserterWooToBiro : IWooToBiro {

        OrderFlow orderFlow;
        IWooToBiroProductInserter productInserter;



        public OrderFlowProductInserterWooToBiro(OrderFlow orderFlow,
            IWooToBiroProductInserter productInserter) {
            
            if (orderFlow == null) {
                throw new ArgumentNullException("orderFlow");
            }
            // productInserter is allowed to be null, sometimes we don't want to insert products

            this.orderFlow = orderFlow;
            this.productInserter = productInserter;
        }


        public async Task<Dictionary<string, object>> OnOrderStatusChanged(string body) {
            return await orderFlow.OnOrderStatusChange(body);
        }

        public async Task<object> OnAttachmentRequest(string body) {
            return await orderFlow.OnAttachmentRequest(body);
        }

        public async Task OnArticleAddedRaw(string product_id, string variation_id) {
            await productInserter.OnArticleAddedRaw(product_id, variation_id);
        }

        public async Task OnArticleChangedRaw(string product_id, string variation_id) {
            await productInserter.OnArticleChangedRaw(product_id, variation_id);
        }

        public async Task OnArticleAdded(WoocommerceProduct product) {
            /* Perhaps for future use
            if (product.Variations == null || product.Variations[0].Attributes.Count == 0)
                throw new Exception("Adding products without variations is not allowed!");

            foreach (var variation in product.Variations) {
                string size = (string)variation.Attributes["attribute_pa_size"];
                string sku = variation.Sku;
                string name = product.Name + " " + size.ToUpper();
                //string ret = await new BirokratSifrantPersistor(client).AddSifrantEntry(name, sku);
                //_logger.LogInformation(ret);
            }
            */
        }

        public async Task OnArticleChanged(Dictionary<string, WoocommerceProduct> product) {
            /* Perhaps for future use
             * var old_prod = product["old"];
            var new_prod = product["new"];
            */
        }
    }
}
