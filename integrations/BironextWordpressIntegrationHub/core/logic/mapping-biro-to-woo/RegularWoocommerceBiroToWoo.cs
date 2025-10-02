using BirokratNext;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.tools.birokratops;
using core.tools.wooops;
using core.tools.zalogaretriever;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_biro_to_woo {


    public class RegularWoocommerceBiroToWoo : IBiroToWoo {

        IApiClientV2 client;
        IOutApiClient wooclient;
        IBirokratArtikelRetriever biroArtikelRetriever;

        BiroToWooSimpleProductSyncer simpleProductSyncer;
        BiroToWooVariableProductSyncer variableProductSyncer;
        BirokratField birokratPropName_of_baseVariationProductSku;
        BirokratField birokratPropName_of_variationProductSku;

        public RegularWoocommerceBiroToWoo(IApiClientV2 client,
            IOutApiClient wooclient,
            BiroToWooSimpleProductSyncer simpleProductSyncer,
            BiroToWooVariableProductSyncer variableProductSyncer,
            IBirokratArtikelRetriever biroArtikelRetriever,
            BirokratField birokratPropName_of_baseVariationProductSku,
            BirokratField birokratPropName_of_variationProductSku) {
            this.client = client;
            this.wooclient = wooclient;
            this.biroArtikelRetriever = biroArtikelRetriever;
            this.simpleProductSyncer = simpleProductSyncer;
            this.variableProductSyncer = variableProductSyncer;
            this.birokratPropName_of_variationProductSku = birokratPropName_of_variationProductSku;
            this.birokratPropName_of_baseVariationProductSku = birokratPropName_of_baseVariationProductSku;
        }

        #region [IBiroToWoo]

        public BirokratField SkuBirokratField { get => birokratPropName_of_variationProductSku; set => throw new NotImplementedException(); }
        public BirokratField VariableProductBirokratField { get => birokratPropName_of_baseVariationProductSku; set => throw new NotImplementedException(); }

        public async Task OnArticleAdded(string sifra) {
            //Console.WriteLine($"Now processing {sifra}");

            string birobasefield = BirokratNameOfFieldInFunctionality.SifrantArtiklov(birokratPropName_of_baseVariationProductSku);

            Dictionary<string, object> biroArtikel = await biroArtikelRetriever.Build(sifra);
            if (string.IsNullOrEmpty((string)biroArtikel[birobasefield])) {
                await simpleProductSyncer.AddProduct(biroArtikel);
            } else {
                await variableProductSyncer.AddProduct(biroArtikel);
            }

        }

        public async Task OnArticleChanged(string sifra) {

            string birobasefield = BirokratNameOfFieldInFunctionality.SifrantArtiklov(birokratPropName_of_baseVariationProductSku);

            Dictionary<string, object> biroArtikel = await biroArtikelRetriever.Build(sifra);
            if (string.IsNullOrEmpty((string)biroArtikel[birobasefield])) {

                await simpleProductSyncer.UpdateProduct(biroArtikel);
            } else {
                await variableProductSyncer.UpdateProduct(biroArtikel);
            }
        }

        public async Task OnArticleDeleted(string sifra) {
            //Delete(sifra);
        }

        public void SetBirokratArtikelRetriever(IBirokratArtikelRetriever zaloga) {
            this.biroArtikelRetriever = zaloga;
        }

        public IBirokratArtikelRetriever GetBirokratArtikelRetriever() {
            return biroArtikelRetriever;
        }

        public Dictionary<string, string> GetVariationAttributes() {
            return variableProductSyncer.GetAttributeMappings();
        }
        #endregion



        #region [auxiliary]
        /*
        private void Delete(string sifra) {
            var woo = wooclient;
            if (string.IsNullOrEmpty(sifra)) return;

            string elementaryProd = sifra.Substring(0, sifra.Length - 2);
            string variationProd = sifra;


            var kmet = GetWooList(woo, $"products?sku={elementaryProd}");

            if (kmet.Count == 0) { // simple product
                var krneki = GetWooList(woo, $"products?sku={variationProd}");
                if (krneki.Count == 0) {
                    Console.WriteLine($"Product not found: {variationProd}");
                    return;
                }
                string prodid = GWooOps.SerializeIntWooProperty(GWooOps.SerializeIntWooProperty(krneki[0]["id"]));
                string oops = woo.Delete($"products/{prodid}");
                Console.WriteLine($"Deleted simple {variationProd}: RESULT = {oops}");
            } else { // variable product
                var tmp = GetWooList(woo, $"products?sku={elementaryProd}");
                if (tmp.Count == 0) {
                    Console.WriteLine($"Product not found: {variationProd}");
                    return;
                }
                string prodid = GWooOps.SerializeIntWooProperty(tmp[0]["id"]);

                tmp = GetWooList(woo, $"products/{prodid}/variations?sku={variationProd}");
                if (tmp.Count == 0) {
                    Console.WriteLine($"Variation not found: {variationProd}");
                    return;
                }
                string varid = GWooOps.SerializeIntWooProperty(tmp[0]["id"]);
                string result = woo.Delete($"products/{prodid}/variations/{varid}");
                Console.WriteLine($"Deleting variation {variationProd}: RESULT = {result}");


                // delete root product if no variations left
                tmp = GetWooList(woo, $"products/{prodid}/variations");
                if (tmp.Count == 0) {
                    woo.Delete($"products/{prodid}");
                }
            }

            return;
        }
        

        private static List<Dictionary<string, object>> GetWooList(IOutApiClient woo, string path) {
            var result = woo.Get(path);
            var some = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<List<Dictionary<string, object>>>(result);
            GWooOps.ThrowExceptionIfProductPostArrayWooApiCallFailed(path, result);
            return some;
        }
        */
        #endregion

    }
}
