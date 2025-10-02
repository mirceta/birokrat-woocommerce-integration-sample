using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using allintegrations_factories;
using BirokratNext;
using BironextWordpressIntegrationHub;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.logic;
using BiroWoocommerceHub.structs_wc_to_biro;
using BiroWoocommerceHubTests;
using BiroWoocommerceHubTests.tools;
using BiroWooHub.logic.integration;
using core.customers.poledancerka;
using core.customers.poledancerka.mappers;
using core.customers.spicasport;
using core.customers.zgeneric;
using core.logic.common_birokrat;
using core.logic.common_woo;
using core.logic.mapping_biro_to_woo;
using core.logic.mapping_biro_to_woo.change_handlers;
using core.logic.mapping_biro_to_woo.syncers;
using core.logic.mapping_biro_to_woo.tools.biro_product_mapping.generic_product_mapping;
using core.logic.mapping_woo_to_biro;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using core.logic.mapping_woo_to_biro.order_operations;
using core.logic.mapping_woo_to_biro.product_ops;
using core.structs;
using core.tools.attributemapper;
using core.tools.birokratops;
using core.tools.wooops;
using core.tools.zalogaretriever;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace allintegrations.customers.poledancerka {
    public class KolosetIntegrationFactory
    {

        public static async Task<IIntegration> BuildIntegration(IApiClientV2 client, IOutApiClient wooclient, string biroApiKey, bool isb2b, string name, IntegrationIdentifier identifier) {

            var zaloga = new RetryingZalogaRetriever(new PerPartesZalogaRetriever(client,
                new Dictionary<string, string>() {
                   { "Centralno", "Cent ralno" }
                }));

            var integ = new RegularIntegration(client, wooclient, biroApiKey, name, null,
                new core.logic.common_birokrat.BirokratObvezneNastavitve(new Dictionary<string, string>() {  }
                ), false,
                        new tests.tools.ValidationComponents(
                            new HardcodedCountryMapper(),
                            new VatNumberParser(),
                            GetTestEqualAdditions(),
                            zaloga))
                            .SetWooToBiro(GetWooToBiro(client, wooclient, isb2b))
                            .SetBiroToWoo(GetBiroToWoo(client, wooclient, zaloga))
                            .SetPhpConfig(new PhpPluginConfig() {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = new List<string>() { "completed" },
                                AttachmentHook = true,
                                OrderStatusHooks = new List<string>() { "processing" }
                            });
            integ.Options.Birotowoo_use_shopify_product_retriever = true;
            //integ.Options["birotowoo_changetracker_dontincludearticlesthatarenotonwebshop"] = "true";

            integ.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(integ, "KOLOSET", null);

            return integ;
        }

        public static IBiroToWoo GetBiroToWoo(IApiClientV2 bironext, IOutApiClient wooclient, IZalogaRetriever zaloga) {

            var changeHandlers = new List<IBirokratProductChangeHandler>();
            changeHandlers.Add(new PriceChangeHandlerWithSalePriceAdjustment());
            changeHandlers.Add(new ZalogaChangeHandler());


            var biroArtikelRetriever = new BirokratArtikelRetriever(bironext, zaloga);

            var genericProductSyncer = new BiroToOutGenericSyncer(wooclient,
                changeHandlers,
                GetProductMapping(),
                BirokratField.SifraArtikla,
                true);

            var inner = new GenericBiroToWoo(bironext,
                wooclient,
                genericProductSyncer,
                biroArtikelRetriever,
                BirokratField.Barkoda4,
                BirokratField.SifraArtikla);

            return inner;
        }

        public static IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient, bool isb2b) {
            return null;
        }

        #region [birotowoo]
        private static IBiroProductToOutMapper GetProductMapping() {
            var mapping = new GenericBiroProductToOutMapper()
                            .SetZaloga(true)
                            .SetTax(GetTaxMapping())
                            .AddVariationDeterminant("Barkoda4")
                            .AddMapping("txtSifraArtikla", "sku")
                            .AddMapping("txtOpis", "name")
                            .AddMapping("PCsPD", "regular_price")
                            .AddAttributeMapping("002_________________DOD", new WooAttr() { Name = "IZBERITE VELIKOST ⬇️" })
                            .AddAttributeMapping("001_________________DOD", new WooAttr() { Name = "BARVA" })
            //.AddCategoryMapping("ComboSkupina")
            ;
            mapping = AddAttributes(mapping);
            return mapping;
        }

        private static BiroTaxToWooTax GetTaxMapping() {
            // TOREJ OD DAVKOV ZAENKRAT PODPIRAMO SAMO TO!!! - VI SI JIH USTVARITE SAMI, SAMO POVEJTE MI KAKO SE BIROKRAT ATRIBUTI PRESLIKAJO!
            BiroTaxToWooTax tax = new BiroTaxToWooTax("SifraDavka", "tax_class");
            tax.AddMapping("1    22 DDV osnovna stopnja", "standard");
            tax.AddMapping("2   9.5 DDV znižana stopnja", "reduced-rate");
            tax.AddMapping("3     0 DDV oproščen promet", "zero-rate");
            tax.AddMapping("4    22 DDV osnovna stopnja            Storitev", "standard");
            tax.AddMapping("5   9.5 DDV znižana stopnja            Storitev", "reduced-rate");
            tax.AddMapping("6     0 DDV oproščen promet            Storitev", "zero-rate");
            tax.AddMapping("7   9.5 Prejemniki plačniki DDV        Storitev", "reduced-rate");
            tax.AddMapping("8     8 Pavšalno nadomestilo           Storitev", ""); // TALE JE KAJ SPLOH?
            tax.AddMapping("9    22 Prejemniki plačniki DDV        Storitev", "standard");
            tax.AddMapping("A     0 DDV drug neobdavč. promet      Storitev", "zero-rate");
            tax.AddMapping("B     0 DDV neobdavčljiv promet        Storitev", "zero-rate");
            tax.AddMapping("C     5 DDV znižana stopnja", ""); // TALE JE KAJ SPLOH?;
            return tax;
        }

        private static IBiroProductToOutMapper AddAttributes(IBiroProductToOutMapper mapping) {
            return mapping
                //.AddAttributeMapping("txtSifraArtikla", new WooAttr() { Name = "Sifra" })
                //.AddAttributeMapping("ComboVrsta", new WooAttr() { Name = "Vrsta" })
                //.AddAttributeMapping("ComboPodVrsta", new WooAttr() { Name = "Podvrsta" })
                //.AddAttributeMapping("1___________________Dodatne last.", new WooAttr() { Name = "Barva" })
                //.AddAttributeMapping("2___________________Dodatne last.", new WooAttr() { Name = "Velikost" });
                //.AddAttributeMapping("3___________________Dodatne last.", new WooAttr() { Name = "Znamka" })
                //.AddAttributeMapping("4___________________Dodatne last.", new WooAttr() { Name = "Spol" })
                //.AddAttributeMapping("5___________________Dodatne last.", new WooAttr() { Name = "Sezona", Visible = false })
                //.AddAttributeMapping("6___________________Dodatne last.", new WooAttr() { Name = "TipProdukta" })
                ;
        }

        private static List<TestEqualAddition> GetTestEqualAdditions() {
            var additionalTests = new List<TestEqualAddition>() {
                /*new TestEqualAddition { biroField = "txtOpis",
                                        outField = "name",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.STRING},
                new TestEqualAddition { biroField = "ComboVrsta",
                                        outField = "Vrsta",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "ComboPodVrsta",
                                        outField = "Podvrsta",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "2___________________Dodatne last.",
                                        outField = "Velikost",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE}*/
            };
            return additionalTests;
        }
        #endregion
    }
}
