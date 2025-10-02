using BiroWoocommerceHubTests;
using BiroWoocommerceHubTests.tools;
using core.logic.common_birokrat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_biro_to_woo.syncers {
    public class BiroToOutGenericSyncer : IBiroToWooProductSyncer {

        IOutApiClient wooclient;
        List<IBirokratProductChangeHandler> changeHandlers;
        IBiroProductToOutMapper mapping;
        BirokratField birokratFieldToSku;
        bool addOnFailToUpdate;

        public BiroToOutGenericSyncer(IOutApiClient wooclient, 
            List<IBirokratProductChangeHandler> changeHandlers,
            IBiroProductToOutMapper mapping, 
            BirokratField birokratFieldToSku, 
            bool addOnFailToUpdate = false) {
                this.wooclient = wooclient;
                this.changeHandlers = changeHandlers;
                this.mapping = mapping;
                this.birokratFieldToSku = birokratFieldToSku;
                this.addOnFailToUpdate = addOnFailToUpdate;
        }

        public async Task AddProduct(Dictionary<string, object> biroArtikel, bool privateProduct = false) {
            var intermediate = await mapping.Map(biroArtikel);
            await wooclient.PostProduct(intermediate);
        }

        public async Task UpdateProduct(Dictionary<string, object> biroArtikel, bool privateProduct = false) {

            string skuField = BirokratNameOfFieldInFunctionality.SifrantArtiklov(birokratFieldToSku);
            string sku = (string)biroArtikel[skuField];

            ProductResult intermediate = null;
            try {
                intermediate = await wooclient.GetProductBySku(sku);
            } catch (Exception ex) { 
            
            }
            if (!intermediate.Success) { // if not found
                if (addOnFailToUpdate) {
                    await AddProduct(biroArtikel, privateProduct);
                    return;
                } else {
                    throw new ProductUpdatingException("Product sku not found!");
                }
            }

            var wooload = new Dictionary<string, object>();
            foreach (var chg in changeHandlers) {
                chg.HandleChange(biroArtikel, intermediate.Product, wooload);
            }

            if (wooload.Keys.Count > 0) {
                await wooclient.UpdateProduct((string)intermediate.Product["sku"], wooload);
            }
        }

        public Dictionary<string, string> GetAttributes() {
            return mapping.GetAttributeMappings();
        }


    }
}
