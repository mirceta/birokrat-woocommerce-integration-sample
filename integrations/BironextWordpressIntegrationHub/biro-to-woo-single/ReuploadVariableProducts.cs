using birowoo_exceptions;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.tools.birokratops;
using core.tools.wooops;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tests;

namespace biro_to_woo_single
{
    partial class Program
    {
        public class WebshopDeleteVarProds_ThenReturnSifras {

            public WebshopDeleteVarProds_ThenReturnSifras() {
            }

            public async Task DeleteProductsRetryingVySku(IIntegration integration, List<string> chome) {
                foreach (string sku in chome) {

                    bool found = true;
                    while (found) {
                        try {
                            await new WooProductDeleter(integration.WooClient).DeleteProductBySku(sku);
                        } catch (ProductNotFoundException ex) {
                            found = false;
                        }
                    }
                }
            }

            public async Task DeleteVariableProductsFromWoocommerce(IIntegration integration) {

                /*
                string products = integration.WooClient.Get($"products?type=variable'&'per_page=100");
                var prod = new JsonPowerDeserialization()
                        .DeserializeObjectImmuneToBadJSONEscapeSequenece<List<Dictionary<string, object>>>(products);
                */
                var prod = await integration.WooClient.GetVariableProducts();




                await new WooProductDeleter(integration.WooClient).DeleteProducts(prod);

            }
        }

        public class BirokratGetVarProds {

            IIntegration integration;

            public BirokratGetVarProds(IIntegration integration) {
                this.integration = integration;
            }

            public async Task<List<string>> GetVariableProductsFromBirokrat() {

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

        
    }
}
