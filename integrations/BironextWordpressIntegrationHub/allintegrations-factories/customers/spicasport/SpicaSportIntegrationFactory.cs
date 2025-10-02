using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using allintegrations_factories;
using ApiClient.utils;
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
using core.customers.zgeneric.order_operations;
using core.logic.common_birokrat;
using core.logic.common_woo;
using core.logic.mapping_biro_to_woo;
using core.logic.mapping_biro_to_woo.change_handlers;
using core.logic.mapping_woo_to_biro;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_extractors;
using core.logic.mapping_woo_to_biro.order_operations;
using core.logic.mapping_woo_to_biro.order_operations.pl;
using core.logic.mapping_woo_to_biro.orderflow.order_operations;
using core.logic.mapping_woo_to_biro.product_ops;
using core.structs;
using core.tools.attributemapper;
using core.tools.birokratops;
using core.tools.wooops;
using core.tools.zalogaretriever;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace allintegrations.customers.spicasport
{
    public class SpicaSportIntegrationFactory {

        bool debug;
        string datafolder;

        public SpicaSportIntegrationFactory(bool debug, string datafolder) {
            this.debug = debug;
            this.datafolder = datafolder;
        }

        public async Task<IIntegration> BuildIntegration(bool wootobiro, IApiClientV2 client, IOutApiClient wooclient, string biroApiKey, string name, IntegrationIdentifier identifier) {

            var zaloga = new RetryingZalogaRetriever(
                new PerPartesZalogaRetriever(client,
                    new Dictionary<string, string>() { { "Centralno", "Cent ralno" } }));

            IWooToBiro wootobiroi = null;
            IBiroToWoo birotowooi = null;
            if (wootobiro)
                wootobiroi = GetWooToBiro(client, wooclient);
            else
                birotowooi = await GetBiroToWoo(client, wooclient, zaloga);
            
            var integ = new RegularIntegration(client, wooclient, biroApiKey, name, identifier,
                new BirokratObvezneNastavitve(new Dictionary<string, string>()), true, 
                new tests.tools.ValidationComponents(
                    new HardcodedCountryMapper(), 
                    new VatNumberParser(),
                    GetTestEqualAdditions(),
                    zaloga))
                            .SetWooToBiro(wootobiroi)
                            .SetBiroToWoo(birotowooi)
                            .SetPhpConfig(new PhpPluginConfig() {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = null,
                                AttachmentHook = false,
                                OrderStatusHooks = new List<string>() { "on-hold", "processing", "completed" }
                            })
                            .SetDatafolder(datafolder);
            
            integ.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(integ, "SPICA", new List<string>() { "processing", "completed" });
            //integ.Options["birotowoo_changetracker_dontincludearticlesthatarenotonwebshop"] = "true";

            return integ;
        }

        public async Task<IBiroToWoo> GetBiroToWoo(IApiClientV2 client, IOutApiClient wooclient, IZalogaRetriever zaloga) {

            var changeHandlers = new List<IBirokratProductChangeHandler>();
            changeHandlers.Add(new PriceChangeHandlerWithSalePriceAdjustment());
            changeHandlers.Add(new ZalogaChangeHandler());

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

            var biroArtikelRetriever = new BirokratArtikelRetriever(client, zaloga);

            IBiroToWoo inner = new RegularWoocommerceBiroToWoo(client, 
                wooclient,
                simpleProductSyncer,
                variableProductSyncer,
                biroArtikelRetriever,
                BirokratField.Barkoda5,
                BirokratField.SifraArtikla);

            return inner;

        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient) {


            // partner vnos TODO
            var statusZavMapper = new B2CStatusPartnerjaMapper();
            var partnerInsert = new ClassicPartnerInserter(client,
                new PartnerWooToBiroMapper1(new HardcodedCountryMapper(), statusZavMapper, statusZavMapper),
                new VatNumberParser());

            // order flow
            var orderflow = new OrderFlow(client, partnerInsert);




            var operations = new List<DocumentParameterCommand>();
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("Sklic")
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Template("SI00 {{{stevilkaDokumenta}}}-1-{{{orderNumber}}}"))
                    .Build());
            var lastStage = new DocumentParametersModifierOrderOperationCR(client, operations, 
                new HardcodedCountryMapper(), !debug ? null : new SaveDocumentOrderOperationCR(client,
                                null, //new ObvestiloOPotekliZalogiOrderOperationCR(client, wooclient, null),
                                datafolder
                            ));

            orderflow.AddOrderFlowStage(
                new OrderCondition() { Status = new List<string>() { "completed" }, PaymentMethod = null },
                new DocumentAlreadyExistsGuard_OrderOperationCR(
                    new DocumentNumberGetter_ByOrderAttributeTemplate2(client, "1-$$$ORDER_NUMBER$$$"), BirokratDocumentType.RACUN,
                    new DocumentInsertionOrderOperationCR(
                        new DocumentInsertion(client,
                                BirokratDocumentType.RACUN,
                                new BirokratAttributeIsOriginalOrVariationSku_BirokratPostavkaExtractor(new BirokratPostavkaUtils(false)),
                                new List<IAdditionalOperationOnPostavke>() {
                                    new Shipping_PostavkaAddOp(client)
                                },
                                null // country mapper!
                        ).SetAdditionalParams((x) => new OrderAdditionalParams() {
                            CountryMapper = null,
                            AdditionalNumber = x.Data.Number,
                            ExternalUniqueIdentifier = x.Data.Number,
                            SourceDocumentType = BirokratDocumentType.DOBAVNICA,
                            SourceDocumentNumberExtractor = new DocumentNumberGetter_ByOrderAttributeTemplate2(client, "1-$$$ORDER_NUMBER$$$")
                        }),
                        BirokratDocumentType.RACUN,
                        new ChangeDocNumOrderOperationCR(client, "1-$$$ORDER_NUMBER$$$",
                            lastStage
                        )
                    )
                )
            );
            orderflow.AddOrderFlowStage(
                new OrderCondition() { Status = new List<string> { "on-hold", "processing" }, PaymentMethod = null },
                new DocumentAlreadyExistsGuard_OrderOperationCR(
                    new DocumentNumberGetter_ByOrderAttributeTemplate2(client, "1-$$$ORDER_NUMBER$$$"), BirokratDocumentType.DOBAVNICA,
                    new DocumentInsertionOrderOperationCR(
                        new DocumentInsertion(client,
                                BirokratDocumentType.DOBAVNICA,
                                new BirokratAttributeIsOriginalOrVariationSku_BirokratPostavkaExtractor(new BirokratPostavkaUtils(false)),
                                new List<IAdditionalOperationOnPostavke>() {
                                    new Shipping_PostavkaAddOp(client)
                                },
                                null // country mapper!
                        ),
                        BirokratDocumentType.DOBAVNICA,
                        new ChangeDocNumOrderOperationCR(client, "1-$$$ORDER_NUMBER$$$",
                            //new ObvestiloOPotekliZalogiOrderOperationCR(client, wooclient, null),
                            lastStage
                        )
                    )
                )
            );
            return new OrderFlowProductInserterWooToBiro(orderflow, null);
        }

        #region [birotowoo]
        private async Task<ArtikelToProductMapping> GetSimpleProductMapping(IApiClientV2 client, IOutApiClient wooclient) {
            var mapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                            .SetType(WooProductType.SIMPLE)
                            .SetZaloga(true)
                            .SetTax(GetTaxMapping())
                            .AddMapping("txtSifraArtikla", "sku")
                            .AddMapping("txtOpis", "name")
                            .AddMapping("txtAlternativniOpis", "description")
                            .AddMapping("PCsPD", "regular_price")
                            .AddCategoryMapping("cmbSkupina");
            mapping = await AddAttributesAsync(mapping);
            return (await ArtikelToProductMapping.NullObject()).SetCustom(mapping);
        }

        private async Task<ArtikelToProductMapping> GetVariableProductBaseMapping(IApiClientV2 client, IOutApiClient wooclient) {
            var mapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                                .SetType(WooProductType.VARIABLE)
                                .SetZaloga(false) // ta je samo osnovni
                                .SetTax(GetTaxMapping())
                                .AddMapping("Barkoda5", "sku")
                                .AddMapping("txtOpis", "name")
                                .AddMapping("txtAlternativniOpis", "description")
                                .AddMapping("PCsPD", "regular_price")
                                .AddCategoryMapping("cmbSkupina");
            mapping = await AddAttributesAsync(mapping);
            return (await ArtikelToProductMapping.NullObject()).SetCustom(mapping);
        }

        private async Task<ArtikelToProductMapping> GetVariableProductVariationMapping(IApiClientV2 client, IOutApiClient wooclient) {
            var varmapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                            .SetZaloga(true)
                            .AddMapping("txtSifraArtikla", "sku")
                            .AddMapping("PCsPD", "regular_price");
            varmapping = await AddAttributesAsync(varmapping);
            return (await ArtikelToProductMapping.NullObject()).SetCustom(varmapping);
        }

        private BiroTaxToWooTax GetTaxMapping() {
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

        public async Task<BirokratArtikelToWooProductMapping> AddAttributesAsync(BirokratArtikelToWooProductMapping mapping)
        {
            mapping = await mapping.AddAttributeMapping("cmbVrsta", new WooAttr() { Name = "Vrsta" });
            mapping = await mapping.AddAttributeMapping("cmbPodvrsta", new WooAttr() { Name = "Podvrsta" });
            mapping = await mapping.AddAttributeMapping("1___________________Dodatne last.", new WooAttr() { Name = "Color" });
            mapping = await mapping.AddAttributeMapping("2___________________Dodatne last.", new WooAttr() { Name = "Size" });
            mapping = await mapping.AddAttributeMapping("3___________________Dodatne last.", new WooAttr() { Name = "Brand" });
            mapping = await mapping.AddAttributeMapping("4___________________Dodatne last.", new WooAttr() { Name = "Sex" });
            mapping = await mapping.AddAttributeMapping("5___________________Dodatne last.", new WooAttr() { Name = "Season", Visible = false });
            mapping = await mapping.AddAttributeMapping("6___________________Dodatne last.", new WooAttr() { Name = "ProductType" });
            mapping = await mapping.AddAttributeMapping("7___________________Dodatne last.", new WooAttr() { Name = "ManufacturerColor" });
            mapping = await mapping.AddAttributeMapping("8___________________Dodatne last.", new WooAttr() { Name = "Material" });
            mapping = await mapping.AddAttributeMapping("10__________________Dodatne last.", new WooAttr() { Name = "Power" });
            mapping = await mapping.AddAttributeMapping("11__________________Dodatne last.", new WooAttr() { Name = "Flavour" });
            mapping = await mapping.AddAttributeMapping("12__________________Dodatne last.", new WooAttr() { Name = "Width" });

            return mapping;
        }


        private List<TestEqualAddition> GetTestEqualAdditions() {
            var additionalTests = new List<TestEqualAddition>() {
                /*new TestEqualAddition { biroField = "txtOpis",
                                        outField = "name",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.STRING},*/
                new TestEqualAddition { biroField = "1___________________Dodatne last.",
                                        outField = "Color",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "2___________________Dodatne last.",
                                        outField = "Size",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "3___________________Dodatne last.",
                                        outField = "Brand",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "4___________________Dodatne last.",
                                        outField = "Sex",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "5___________________Dodatne last.",
                                        outField = "Season",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "6___________________Dodatne last.",
                                        outField = "ProductType",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "7___________________Dodatne last.",
                                        outField = "ManufacturerColor",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "8___________________Dodatne last.",
                                        outField = "Material",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "10__________________Dodatne last.",
                                        outField = "Power",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "11__________________Dodatne last.",
                                        outField = "Flavour",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
                new TestEqualAddition { biroField = "12__________________Dodatne last.",
                                        outField = "Width",
                                        outType = OutType.WOOCOMMERCE,
                                        articleType = ArticleType.BOTH,
                                        outFieldType = OutFieldType.VARIABLE_ATTRIBUTE},
            };
            return additionalTests;
        }
        #endregion
    }
}
