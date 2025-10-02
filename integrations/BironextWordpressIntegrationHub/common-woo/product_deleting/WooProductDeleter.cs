using biro_to_woo.logic.change_trackers.exhaustive;
using birowoo_exceptions;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.tools.birokratops;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace tests
{
    public class WebshopDeleteVarProds_ThenReturnSifras
    {

        public WebshopDeleteVarProds_ThenReturnSifras()
        {
        }

        public async Task DeleteProductsRetryingBySku(IIntegration integration, List<string> skus, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            foreach (string sku in skus)
            {

                bool found = true;
                while (found)
                {
                    var result = await new WooProductDeleter(integration.WooClient).DeleteProductBySku(sku);
                    if (!result.Success && result.ErrorMessage.Contains("not found"))
                    {
                        found = false;
                    }
                }
            }
        }

        public async Task DeleteVariableProductsFromWoocommerce(IIntegration integration)
        {

            /*
            string products = integration.WooClient.Get($"products?type=variable'&'per_page=100");
            var prod = new JsonPowerDeserialization()
                    .DeserializeObjectImmuneToBadJSONEscapeSequenece<List<Dictionary<string, object>>>(products);
            */
            var prod = integration.WooClient.GetVariableProducts().GetAwaiter().GetResult();




            await new WooProductDeleter(integration.WooClient).DeleteProducts(prod);

        }
    }

    public class BirokratGetVarProds
    {

        IIntegration integration;

        public BirokratGetVarProds(IIntegration integration)
        {
            this.integration = integration;
        }

        public async Task<List<string>> GetVariableProductsFromBirokrat()
        {

            var variableProductField = integration.BiroToWoo.VariableProductBirokratField;
            string varprodfield = BirokratNameOfFieldInFunctionality
                        .KumulativaPodrobniPregledArtiklov(variableProductField);

            var tmp = await new PodrobniPregledArtiklov().GetPodrobniPregledArtiklov(integration.BiroClient);
            tmp = tmp.Where(x => (string)x["Prenesi v e-shop"] == "-1").ToList();

            var variable = tmp
                    .Where(x => !string.IsNullOrEmpty((string)x[varprodfield]))
                    .ToList();
            var sifre = variable.Select(x => (string)x["Artikel"]).ToList();
            return sifre;
        }
    }

    public class WooProductDeleter : WebshopProductDeleter
    {

        IOutApiClient woo;

        int parallelTaskCount = 5;

        public WooProductDeleter(IOutApiClient caller) {
            woo = caller;
        }

        public async Task DeleteAllProducts() {
            throw new NotImplementedException("Not implemented!");
        }

        public async Task DeleteProductById(string id) {
            throw new NotImplementedException();
        }

        public async Task<DeletionResult> DeleteProductBySku(string sku)
        {
            var result = await woo.GetProductBySku(sku);

            if (!result.Success)
            {
                return DeletionResult.FailureResult(result.ErrorMessage);
            }

            var prod = result.Product;

            string parent_id = GWooOps.SerializeIntWooProperty(prod["parent_id"]);

            await DeleteProducts(new List<Dictionary<string, object>>() { prod });

            if (!string.IsNullOrEmpty(parent_id) && parent_id != "0")
            {
                var parentResult = await woo.GetProductBySku(sku);
                if (parentResult.Success)
                {
                    var parent = parentResult.Product;
                    var variations = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(prod["variations"]));
                    if (variations.Count == 0)
                    {
                        await DeleteProduct(parent);
                    }
                }
            }

            return DeletionResult.SuccessResult();
        }


        public async Task DeleteProducts(List<Dictionary<string, object>> products) {
            foreach (var vari in products) {
                await DeleteProduct(vari);
            }
        }

        private async Task DeleteProduct(Dictionary<string, object> vari) {
            string parent_id = GWooOps.SerializeIntWooProperty(vari["parent_id"]);
            if (!string.IsNullOrEmpty(parent_id) && parent_id != "0") {
                await woo.DeleteVariation(GWooOps.SerializeIntWooProperty(vari["parent_id"]), GWooOps.SerializeIntWooProperty(vari["id"]));
            } else {
                await woo.DeleteProduct(GWooOps.SerializeIntWooProperty(vari["id"]));
            }
        }
    }

    public class DeletionResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public static DeletionResult SuccessResult()
        {
            return new DeletionResult { Success = true };
        }

        public static DeletionResult FailureResult(string errorMessage)
        {
            return new DeletionResult { Success = false, ErrorMessage = errorMessage };
        }
    }

}