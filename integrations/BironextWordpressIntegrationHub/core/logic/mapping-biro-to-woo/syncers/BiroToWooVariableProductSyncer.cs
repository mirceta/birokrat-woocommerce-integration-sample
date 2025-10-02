using birowoo_exceptions;
using BiroWoocommerceHubTests;
using BiroWoocommerceHubTests.tools;
using core.logic.common_birokrat;
using core.logic.mapping_biro_to_woo.syncers;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_biro_to_woo
{
    public class BiroToWooVariableProductSyncer : IBiroToWooProductSyncer
    {
        IOutApiClient wooclient;
        List<IBirokratProductChangeHandler> changeHandlers;
        ArtikelToProductMapping variableProductVariationMapping;
        ArtikelToProductMapping variableProductBaseMapping;

        BirokratField birokratPropName_of_baseVariationProductSku;
        BirokratField birokratPropName_of_variationSku;

        bool addOnFailToUpdate;
        bool requireBaseSkuAttrPrefixOfVariationSkuAttr;
        public BiroToWooVariableProductSyncer(IOutApiClient wooclient,
            List<IBirokratProductChangeHandler> changeHandlers,
            ArtikelToProductMapping variableProductBaseMapping,
            ArtikelToProductMapping variableProductVariationMapping,
            BirokratField birokratPropName_of_baseVariationProductSku = BirokratField.Barkoda5,
            BirokratField birokratPropName_of_variationSku = BirokratField.SifraArtikla,
            bool addOnFailToUpdate = false,
            bool requireBaseSkuAttrPrefixOfVariationSkuAttr = true)
        {

            this.birokratPropName_of_baseVariationProductSku = birokratPropName_of_baseVariationProductSku;
            this.birokratPropName_of_variationSku = birokratPropName_of_variationSku;

            this.wooclient = wooclient;
            this.changeHandlers = changeHandlers;
            this.variableProductBaseMapping = variableProductBaseMapping;
            this.variableProductVariationMapping = variableProductVariationMapping;

            this.addOnFailToUpdate = addOnFailToUpdate;
            this.requireBaseSkuAttrPrefixOfVariationSkuAttr = requireBaseSkuAttrPrefixOfVariationSkuAttr;
        }

        public async Task AddProduct(Dictionary<string, object> biroArtikel, bool privateProduct = false) {

            if (requireBaseSkuAttrPrefixOfVariationSkuAttr && !IsBarcodePrefixOfSifraArtikla(biroArtikel))
                throw new ProductAddingException($"Barcode must be prefix of sifra artikla {(string)biroArtikel["txtSifraArtikla"]}");

            string birofield = BirokratNameOfFieldInFunctionality.SifrantArtiklov(birokratPropName_of_baseVariationProductSku);

            string productId = "";
            string sku = (string)biroArtikel[birofield];

            Dictionary<string, object> root = null;
            try {
                var tmp1 = (await wooclient.GetProductBySku(sku));
                if (!tmp1.Success && tmp1.ErrorMessage.Contains("not found")) { }
                else
                {
                    root = tmp1.Product;
                    productId = GWooOps.SerializeIntWooProperty(root["id"]);
                }
            } catch (ProductInDraftStatusException ex) {
                throw ex; // if the product is a draft, then we cannot publish its variation!
            } catch (MultipleProductVariationsWithSameSku ex) {
                throw ex; // if there are multiple active products with this sku, then we cannot publish!
            } catch (ProductNotFoundException ex) { 
                // here do nothing since not finding it is completely fine.
            }


            var woo = await variableProductVariationMapping.CompleteVariationMapping(biroArtikel, productId, variableProductBaseMapping);
            productId = (string)woo["parent_id"];
            
            var tmp = await wooclient.PostVariation(productId, woo);
            string json = JsonConvert.SerializeObject(woo);
            string res = JsonConvert.SerializeObject(tmp);




            ProductSyncerHelper.ValidateChanges(biroArtikel, json, res);
        }

        public async Task UpdateProduct(Dictionary<string, object> biroArtikel, bool privateProduct = false)
        {
            if (requireBaseSkuAttrPrefixOfVariationSkuAttr && !IsBarcodePrefixOfSifraArtikla(biroArtikel))
                    throw new ProductUpdatingException($"Barcode must be prefix of sifra artikla {(string)biroArtikel["txtSifraArtikla"]}");
            string birobasefield = BirokratNameOfFieldInFunctionality.SifrantArtiklov(birokratPropName_of_baseVariationProductSku);
            string birovarfield = BirokratNameOfFieldInFunctionality.SifrantArtiklov(birokratPropName_of_variationSku);
            string sifraOsnovni = (string)biroArtikel[birobasefield];
            string sifraVariacije = (string)biroArtikel[birovarfield];
            
            string productId = "";
            Dictionary<string, object> root = null;
            try {
                var tmp1 = await wooclient.GetProductBySku(sifraOsnovni);
                if (tmp1.Success)
                {
                    root = tmp1.Product;
                    productId = GWooOps.SerializeIntWooProperty(root["id"]);
                }
            } catch (ProductInDraftStatusException ex) {
                throw ex; // if the product is a draft, then we cannot publish its root!
            } catch (MultipleProductVariationsWithSameSku ex) {
                throw ex; // if there are multiple active products with this sku, then we cannot publish!
            } catch (ProductNotFoundException ex) {
                // here do nothing since not finding it is completely fine.
            }


            if (productId == "") {
                if (addOnFailToUpdate) {
                    await AddProduct(biroArtikel, privateProduct);
                    return;
                } else {
                    throw new ProductUpdatingException("Product sku not found!");
                }
            }
            
            Dictionary<string, object> variation = null;
            try {
                var tmp2 = await wooclient.GetProductBySku(sifraVariacije);
                if (tmp2.Success)
                {
                    variation = tmp2.Product;
                    productId = GWooOps.SerializeIntWooProperty(root["id"]);
                }
                else if (!tmp2.Success && tmp2.ErrorMessage.Contains("not found")) {
                    if (addOnFailToUpdate)
                    {
                        await AddProduct(biroArtikel, privateProduct);
                        return;
                    }
                    else
                    {
                        throw new ProductUpdatingException("Product variation not found!");
                    }
                }   
            } catch (ProductInDraftStatusException ex) {
                throw ex; // if the product is a draft, then we cannot publish its variation!
            } catch (MultipleProductVariationsWithSameSku ex) {
                throw ex; // if there are multiple active products with this sku, then we cannot publish!
            }

            var wooload = new Dictionary<string, object>();
            foreach (var chg in changeHandlers)
            {
                chg.HandleChange(biroArtikel, variation, wooload);
            }

            if (wooload.Keys.Count > 0) {
                string body, res;
                VariationUploadChanges(productId, variation, wooload, out body, out res);
                ProductSyncerHelper.ValidateChanges(biroArtikel, body, res);
            }
        }

        public Dictionary<string, string> GetAttributeMappings() {
            return variableProductVariationMapping.GetAttributeMappings();
        }

        #region [auxiliary]
        
        private string VariationUploadChanges(string productId, Dictionary<string, object> variation, Dictionary<string, object> wooload, out string body, out string res) {

            /*
            body = JsonConvert.SerializeObject(wooload);
            string variationid = GWooOps.SerializeIntWooProperty(variation["id"]);
            res = wooclient.Put($"products/{productId}/variations/{variationid}", body);
            */

            string variationid = GWooOps.SerializeIntWooProperty(variation["id"]);
            var result = wooclient.UpdateVariation(productId, variationid, wooload).GetAwaiter().GetResult();
            res = JsonConvert.SerializeObject(result);
            body = JsonConvert.SerializeObject(wooload);





            return res;
        }

        private bool IsBarcodePrefixOfSifraArtikla(Dictionary<string, object> biroArtikel) {
            string birobasefield = BirokratNameOfFieldInFunctionality.SifrantArtiklov(birokratPropName_of_baseVariationProductSku);
            string birovarfield = BirokratNameOfFieldInFunctionality.SifrantArtiklov(birokratPropName_of_variationSku);
            return ((string)biroArtikel[birovarfield]).Contains((string)biroArtikel[birobasefield]);
        }
        #endregion
    }
}
