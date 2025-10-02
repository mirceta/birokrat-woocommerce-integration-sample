using birowoo_exceptions;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHubTests;
using BiroWoocommerceHubTests.tools;
using core.logic.common_birokrat;
using core.logic.mapping_biro_to_woo.syncers;
using core.structs;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_biro_to_woo
{
    public class BiroToWooSimpleProductSyncer : IBiroToWooProductSyncer
    {

        IOutApiClient wooclient;
        List<IBirokratProductChangeHandler> changeHandlers;
        ArtikelToProductMapping mapping;
        BirokratField birokratFieldToSku;
        bool addOnFailToUpdate;
        public BiroToWooSimpleProductSyncer(IOutApiClient wooclient, 
            List<IBirokratProductChangeHandler> changeHandlers, 
            ArtikelToProductMapping mapping, 
            BirokratField birokratFieldToSku, 
            bool addOnFailToUpdate = false) {
            this.wooclient = wooclient;
            this.changeHandlers = changeHandlers;
            this.mapping = mapping;
            this.birokratFieldToSku = birokratFieldToSku;
            this.addOnFailToUpdate = addOnFailToUpdate;
            
        }

        public async Task AddProduct(Dictionary<string, object> biroArtikel, bool privateProduct = false)
        {
            var woo = await mapping.CompleteProductMapping(biroArtikel);
            if (privateProduct) {
                woo["status"] = "private";
            }
            string woojson = JsonConvert.SerializeObject(woo);
            woo["status"] = "draft";
            var tmp = await wooclient.PostProduct(woo);
            string result = JsonConvert.SerializeObject(tmp);

            
            ProductSyncerHelper.ValidateChanges(biroArtikel, woojson, result);
        }

        public async Task UpdateProduct(Dictionary<string, object> biroArtikel, bool privateProduct = false) {

            string skuField = BirokratNameOfFieldInFunctionality.SifrantArtiklov(birokratFieldToSku);

            string sku = (string)biroArtikel[skuField];
            string productId = "";
            Dictionary<string, object> wooobj = null;
            try
            {
                var tmp1 = (await wooclient.GetProductBySku(sku));
                if (!tmp1.Success && tmp1.ErrorMessage.Contains("not found")) { }
                else
                {
                    wooobj = tmp1.Product;
                    productId = GWooOps.SerializeIntWooProperty(wooobj["id"]);
                }
            }
            catch (ProductInDraftStatusException ex)
            {
                throw ex; // if the product is a draft, then we cannot publish its variation!
            }
            catch (MultipleProductVariationsWithSameSku ex)
            {
                throw ex; // if there are multiple active products with this sku, then we cannot publish!
            }
            catch (ProductNotFoundException ex)
            {
                // here do nothing since not finding it is completely fine.
            }

            if (wooobj == null) {
                if (addOnFailToUpdate) {
                    await AddProduct(biroArtikel, privateProduct);
                    return;
                } else {
                    throw new ProductUpdatingException("Product sku not found!");
                }
            }
            

            var wooload = new Dictionary<string, object>();
            foreach (var chg in changeHandlers) {
                chg.HandleChange(biroArtikel, wooobj, wooload);
            }
            if (wooload.Keys.Count > 0) {
                string body, res;
                UploadChanges(productId, wooobj, wooload, out body, out res);
                ProductSyncerHelper.ValidateChanges(biroArtikel, body, res);
            }
        }
    
        private void UploadChanges(string productId, Dictionary<string, object> wooobj, Dictionary<string, object> wooload, out string body, out string res) {

            
            
            body = JsonConvert.SerializeObject(wooload);
            Dictionary<string, object> tmp = null;
            bool parentIdNotNull = false;
            if (wooobj["parent_id"] != null) {
                string parentid = GWooOps.SerializeIntWooProperty(wooobj["parent_id"]);
                if (parentid != null && parentid.Length > 1) {
                    parentIdNotNull = true;
                    tmp = wooclient.UpdateVariation(parentid, productId, wooload).GetAwaiter().GetResult();
                }
            } 
            if (!parentIdNotNull) {
                tmp = wooclient.UpdateProduct(productId, wooload).GetAwaiter().GetResult();
            }
            res = JsonConvert.SerializeObject(tmp);
        }

    }
}
