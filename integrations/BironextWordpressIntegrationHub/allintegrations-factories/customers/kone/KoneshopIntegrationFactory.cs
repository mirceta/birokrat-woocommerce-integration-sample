using allintegrations_factories.wrappers;
using ApiClient.utils;
using BirokratNext;
using BiroWoocommerceHub;
using BiroWoocommerceHub.structs_wc_to_biro;
using BiroWoocommerceHubTests;
using BiroWoocommerceHubTests.tools;
using BiroWooHub.logic.integration;
using core.customers.spicasport;
using core.customers.zgeneric;
using core.logic.common_birokrat;
using core.logic.common_woo;
using core.logic.mapping_biro_to_woo;
using core.logic.mapping_biro_to_woo.change_handlers;
using core.logic.mapping_biro_to_woo.syncers;
using core.logic.mapping_biro_to_woo.tools.biro_product_mapping.generic_product_mapping;
using core.structs;
using core.tools.attributemapper;
using core.tools.zalogaretriever;
using JsonIntegrationLoader.utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using tests.tools;

namespace allintegrations.customers.poledancerka
{
    public class KoneshopIntegrationFactory
    {

        ValidationComponents validationComponents;
        bool debug;
        string datafolder;

        public KoneshopIntegrationFactory(bool debug, string datafolder) { 
            this.debug = debug;
            this.datafolder = datafolder;
        }

        public async Task<IIntegration> BuildIntegration(IApiClientV2 client, IOutApiClient wooclient, 
            string biroApiKey, bool isb2b, string name, IntegrationIdentifier identifier)
        {

            var zaloga = new RetryingZalogaRetriever(new PerPartesZalogaRetriever(client,
                new Dictionary<string, string>() {
                   { "Centralno", "Cent ralno" }
                }));


            var variationalAttributesBuilder = new VariationalAttributesBuilder();
            validationComponents = new ValidationComponents(
                    new HardcodedCountryMapper(),
                    new VatNumberParser(),
                    variationalAttributesBuilder.GetTestEqualAdditions(),
                    null);

            var integ = new RegularIntegration(client, wooclient, biroApiKey, name, null,
                new core.logic.common_birokrat.BirokratObvezneNastavitve(new Dictionary<string, string>() { }
                ), false,
                        new tests.tools.ValidationComponents(
                            new HardcodedCountryMapper(),
                            new VatNumberParser(),
                            GetTestEqualAdditions(),
                            zaloga))
                            .SetWooToBiro(GetWooToBiro(client, wooclient))
                            .SetBiroToWoo(await GetBiroToWoo(client, wooclient, zaloga))
                            .SetPhpConfig(new PhpPluginConfig()
                            {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = new List<string>() { },
                                AttachmentHook = true,
                                OrderStatusHooks = new List<string>() { "on-hold", "processing" }
                            });
            
            integ.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(integ, "", null);

            return integ;
        }

        public static async Task<IBiroToWoo> GetBiroToWoo(IApiClientV2 client, IOutApiClient wooclient, IZalogaRetriever zaloga)
        {

            var changeHandlers = new List<IBirokratProductChangeHandler>();
            changeHandlers.Add(new PriceChangeHandlerWithSalePriceAdjustment());
            changeHandlers.Add(new ZalogaChangeHandler());

            var biroArtikelRetriever = new BirokratArtikelRetriever(client, zaloga);

            var simpleProductSyncer = new BiroToWooSimpleProductSyncer(wooclient,
                   changeHandlers,
                   await GetSimpleProductMapping(client, wooclient),
                   BirokratField.SifraArtikla,
                   true);
            var variableProductSyncer = new BiroToWooVariableProductSyncer(wooclient,
                changeHandlers,
                await GetVariableProductBaseMapping(client, wooclient),
                await GetVariableProductVariationMapping(client, wooclient),
                BirokratField.Barkoda5,
                BirokratField.SifraArtikla,
                true);

            IBiroToWoo inner = new RegularWoocommerceBiroToWoo(client,
                wooclient,
                simpleProductSyncer,
                variableProductSyncer,
                biroArtikelRetriever,
            BirokratField.Barkoda5,
                BirokratField.SifraArtikla);

            return inner;
        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient)
        {
            var builder = new OrderFlowStageBuilder(false, client,
                new HardcodedCountryMapper(),
                datafolder);

            builder.SetValidationComponents(validationComponents);

            builder.SetPostavkeOperations(includePostavkeComments: true,
                percentCoupons: true,
                fixedCartCoupons: true,
                includeShipping: true,
                includeHandlingOproscenaDobava: false);
            builder.BuildOrderFlow();

            var docNumTemplate = new OrderAttributeTemplateParser2(
                    "1-$$$PAYMENT_METHOD$$$-$$$ORDER_NUMBER$$$",
                    new OrderAttributeTemplateParserDecorator(
                        new Dictionary<string, string>() {
                            { "cod", "1" },
                            { "bacs", "2" },
                            { "ppcp-gateway", "3"}
                        })
                    );

            builder.AddStage(
                overrideSklicWithAdditionalNumber: true,
                orderCondition: new OrderCondition()
                {
                    Status = new List<string>() { "on-hold", "processing" },
                    PaymentMethod = null
                },
                doctype: BirokratDocumentType.RACUN,
                sourceDocType: BirokratDocumentType.UNASSIGNED,
                parser: docNumTemplate
            );

            var orderflow = builder.GetOrderFlow();
            return new OrderFlowProductInserterWooToBiro(orderflow, null);
        }

        #region [birotowoo]
        private static async Task<ArtikelToProductMapping> GetSimpleProductMapping(IApiClientV2 client, IOutApiClient wooclient)
        {
            var mapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                            .SetType(WooProductType.SIMPLE)
                            .SetZaloga(true)
                            .SetTax(GetTaxMapping())
                            .AddMapping("txtSifraArtikla", "sku")
                            .AddMapping("txtOpis", "name")
                            .AddMapping("txtAlternativniOpis", "description")
                            .AddMapping("PCsPD", "regular_price");
            mapping = await AddAttributes(mapping);
            return (await ArtikelToProductMapping.NullObject()).SetCustom(mapping);
        }

        private static async Task<ArtikelToProductMapping> GetVariableProductBaseMapping(IApiClientV2 client, IOutApiClient wooclient)
        {
            var varmapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                            .SetZaloga(true)
                            .SetType(WooProductType.VARIABLE)
                            .SetTax(GetTaxMapping())
                            .AddMapping("Barkoda5", "sku")
                            .AddMapping("txtOpis", "name")
                            .AddMapping("txtAlternativniOpis", "description")
                            .AddMapping("PCsPD", "regular_price");
            varmapping = await AddAttributes(varmapping);
            return (await ArtikelToProductMapping.NullObject()).SetCustom(varmapping);
        }

        private static async Task<ArtikelToProductMapping> GetVariableProductVariationMapping(IApiClientV2 client, IOutApiClient wooclient)
        {
            var varmapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                            .SetZaloga(true)
                            .AddMapping("txtSifraArtikla", "sku")
                            .AddMapping("PCsPD", "regular_price");
            varmapping = await AddAttributes(varmapping);
            return (await ArtikelToProductMapping.NullObject()).SetCustom(varmapping);
        }

        private static async Task<BirokratArtikelToWooProductMapping> AddAttributes(BirokratArtikelToWooProductMapping mapping)
        {
            return await mapping
               .AddAttributeMapping("01__________________BARVE", new WooAttr() { Name = "BARVA" });
        }

        private static BiroTaxToWooTax GetTaxMapping()
        {
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

        private static List<TestEqualAddition> GetTestEqualAdditions()
        {
            var additionalTests = new List<TestEqualAddition>()
            {
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