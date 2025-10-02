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
using core.logic.mapping_woo_to_biro.product_ops;
using core.structs;
using core.tools.attributemapper;
using core.tools.birokratops;
using core.tools.wooops;
using core.tools.zalogaretriever;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace allintegrations.customers.ekodrive
{
    public class EkodriveIntegrationFactory {

        bool debug;
        string datafolder;
        public EkodriveIntegrationFactory(bool debug, string datafolder) {
            this.debug = debug;
            this.datafolder = datafolder;
        }

        public async Task<IIntegration> BuildIntegration(IApiClientV2 client, IOutApiClient wooclient, string biroApiKey, string name) {

            var zaloga = new RetryingZalogaRetriever(new PerPartesZalogaRetriever(client,
                    new Dictionary<string, string>() { { "Centralno", "Cent ralno" } }));

            var mapper = new WooToBiroCountryMapper(client);
            mapper.Setup();

            var integ = new RegularIntegration(client, wooclient, biroApiKey, 
                name, null, 
                new BirokratObvezneNastavitve(new Dictionary<string, string>()), false, 
                new tests.tools.ValidationComponents(mapper, new NopVatParser(), null, zaloga))
                            .SetWooToBiro(GetWooToBiro(client, wooclient, mapper))
                            .SetBiroToWoo(await GetBiroToWoo(client, wooclient, zaloga))
                            .SetPhpConfig(new PhpPluginConfig() {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = new List<string>() { "on-hold", "processing", "completed" },
                                AttachmentHook = true,
                                OrderStatusHooks = new List<string>() { "on-hold", "processing", "completed" }
                            })
                            .SetDatafolder(datafolder);
            integ.Options.Birotowoo_changetracker_dontincludearticlesthatarenotonwebshop = true;

            integ.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(integ, "DEFAULT", new List<string>() { "processing", "completed" });

            return integ;

        }

        public async Task<IBiroToWoo> GetBiroToWoo(IApiClientV2 client, IOutApiClient wooclient, IZalogaRetriever zaloga) {

            var changeHandlers = new List<IBirokratProductChangeHandler>();
            changeHandlers.Add(new PriceChangeHandlerWithSalePriceAdjustment());
            changeHandlers.Add(new ZalogaChangeHandler());

            
            var simpleProductSyncer = new BiroToWooSimpleProductSyncer(wooclient, 
                changeHandlers, 
                await GetSimpleProductMapping(client, wooclient), 
                BirokratField.Barkoda3,
                false);
            var variableProductSyncer = new BiroToWooVariableProductSyncer(wooclient, changeHandlers,
                await GetVariableProductBaseMapping(client, wooclient),
                await GetVariableProductVariationMapping(client, wooclient),
                BirokratField.Barkoda5,
                BirokratField.Barkoda3,
                false);

            
            var biroArtikelRetriever = new BirokratArtikelRetriever(client, zaloga);

            return new RegularWoocommerceBiroToWoo(client,
                wooclient,
                simpleProductSyncer,
                variableProductSyncer,
                biroArtikelRetriever,
                BirokratField.Barkoda5,
                BirokratField.Barkoda3);
        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient, ICountryMapper countryMapper) {

            

            var statusZavMapper = new B2CStatusPartnerjaMapper();
            var partnerInsert = new ClassicPartnerInserter(client, 
                new PartnerWooToBiroMapper1(countryMapper, statusZavMapper, statusZavMapper),
                new VatNumberParser());

            var orderFlow = new OrderFlow(client, partnerInsert);

            AddEkodriveDocumentFlowStage(orderFlow,
                new OrderCondition() { Status = new List<string>() { "on-hold" }, PaymentMethod = new List<string> { "cod", "bacs" } }, // po povzetju, po predracunu
                BirokratDocumentType.PREDRACUN, client);
            AddEkodriveDocumentWithSourceDocumentFlowStage(orderFlow,
                new OrderCondition() { Status = new List<string>() { "completed" }, PaymentMethod = new List<string> { "cod" } },
                BirokratDocumentType.RACUN, BirokratDocumentType.PREDRACUN, client);
            AddEkodriveDocumentWithSourceDocumentFlowStage(orderFlow,
                new OrderCondition() { Status = new List<string>() { "processing" }, PaymentMethod = new List<string> { "bacs" } },
                BirokratDocumentType.RACUN, BirokratDocumentType.PREDRACUN, client);



            AddEkodriveDocumentFlowStage(orderFlow,
                new OrderCondition() { Status = new List<string>() { "on-hold" }, PaymentMethod = new List<string> { "bankart_payment_gateway_diners_cards", "SaferpayCw_CreditCard" } },
                BirokratDocumentType.RACUN, client);
            AddEkodriveDocumentFlowStage(orderFlow,
                new OrderCondition() { Status = new List<string>() { "on-hold" }, PaymentMethod = new List<string> {  "wc_leanpay" } },
                BirokratDocumentType.RACUN, client);





            /*
            AddEkodriveAttachmentFlowStage(orderFlow,
                new OrderCondition() { Status = new List<string> { "on-hold" }, PaymentMethod = new List<string> { "cod", "bacs" } },
                BirokratDocumentType.PREDRACUN, client);
            AddEkodriveAttachmentFlowStage(orderFlow,
                new OrderCondition() { Status = new List<string> { "completed" }, PaymentMethod = new List<string> { "cod" } },
                BirokratDocumentType.RACUN, client);
            AddEkodriveAttachmentFlowStage(orderFlow,
                new OrderCondition() { Status = new List<string> { "processing" }, PaymentMethod = new List<string> { "bacs" } },
                BirokratDocumentType.RACUN, client);
            AddEkodriveAttachmentFlowStage(orderFlow,
                new OrderCondition() { Status = new List<string> { "on-hold" }, PaymentMethod = new List<string> { "bankart_payment_gateway_diners_cards", "wc_leanpay", "SaferpayCw_CreditCard" } },
                BirokratDocumentType.RACUN, client);
            AddEkodriveAttachmentFlowStage(orderFlow,
                new OrderCondition() { Status = new List<string> { "on-hold" }, PaymentMethod = new List<string> { "wc_leanpay" } },
                BirokratDocumentType.RACUN, client);
            */

            return new OrderFlowProductInserterWooToBiro(orderFlow, null);
        }

        #region [wootobiro]
        void AddEkodriveDocumentWithSourceDocumentFlowStage(OrderFlow oflow, OrderCondition oc, BirokratDocumentType documentType, BirokratDocumentType sourceDocumentType, IApiClientV2 bironext) {
            oflow.AddOrderFlowStage(oc,
                new DocumentInsertionOrderOperationCR(
                     new DocumentInsertion(bironext,
                             documentType,
                             new BirokratAttributeIsOriginalOrVariationSku_BirokratPostavkaExtractor(new BirokratPostavkaUtils(false)),
                             new List<IAdditionalOperationOnPostavke>() {
                                new CouponPercent_PostavkeAddOp(),
                                new CouponFixedCart_PostavkeAddOp(bironext),
                                new Shipping_PostavkaAddOp(bironext),
                                new ProvizijaZaOdkupnino_PostavkaAddOp(bironext, "1,50")
                             },
                             null // country mapper!
                     ).SetAdditionalParams((x) => new OrderAdditionalParams() {
                         CountryMapper = null,
                         AdditionalNumber = "1-" + x.Data.Number,
                         ExternalUniqueIdentifier = x.Data.Number,
                         SourceDocumentType = sourceDocumentType,
                         SourceDocumentNumberExtractor = new DocumentNumberGetter_ByOrderAttributeTemplate2(bironext, "1-$$$ORDER_NUMBER$$$")
                     }),
                     documentType,
                     !debug ? null : new SaveDocumentOrderOperationCR(bironext,
                            null,
                            datafolder)
                 ));
        }

        void AddEkodriveDocumentFlowStage(OrderFlow oflow, OrderCondition oc, BirokratDocumentType documentType, IApiClientV2 bironext) {
            oflow.AddOrderFlowStage(oc,
                new DocumentInsertionOrderOperationCR(
                     new DocumentInsertion(bironext,
                             documentType,
                             new BirokratAttributeIsOriginalOrVariationSku_BirokratPostavkaExtractor(new BirokratPostavkaUtils(false)),
                             new List<IAdditionalOperationOnPostavke>() {
                                new CouponPercent_PostavkeAddOp(),
                                new CouponFixedCart_PostavkeAddOp(bironext),
                                new Shipping_PostavkaAddOp(bironext),
                                new ProvizijaZaOdkupnino_PostavkaAddOp(bironext, "1,50")
                             },
                             null // country mapper!
                     ).SetAdditionalParams((x) => new OrderAdditionalParams() {
                         CountryMapper = null,
                         AdditionalNumber = "1-" + x.Data.Number,
                         ExternalUniqueIdentifier = x.Data.Number,
                         SourceDocumentType = BirokratDocumentType.UNASSIGNED,
                         SourceDocumentNumberExtractor = null
                     }),
                     documentType,
                     !debug ? null : new SaveDocumentOrderOperationCR(bironext,
                            null,
                            datafolder)
                 ));
        }

        void AddEkodriveAttachmentFlowStage(OrderFlow oflow, OrderCondition oc, BirokratDocumentType documentType, IApiClientV2 client) {
            oflow.AddAttachmentFlowStage(
                 oc,
                 new DocumentNumberGetter_ByOrderAttributeTemplate(client, "1-$$$ORDER_NUMBER$$$", documentType,
                     new BiroDocumentPdfGetter(client, null)));
        }
        #endregion
        #region [birotowoo]
        private async Task<ArtikelToProductMapping> GetSimpleProductMapping(IApiClientV2 client, IOutApiClient wooclient) {
            var mapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                            .SetType(WooProductType.SIMPLE)
                            .SetZaloga(true)
                            .SetTax(GetTaxMapping())
                            .AddMapping("Barkoda3", "sku")
                            .AddMapping("txtOpis", "name")
                            .AddMapping("PCsPD", "regular_price")
                            .AddCategoryMapping("Barkoda4");
            return (await ArtikelToProductMapping.NullObject()).SetCustom(mapping);
        }

        private async Task<ArtikelToProductMapping> GetVariableProductBaseMapping(IApiClientV2 client, IOutApiClient wooclient) {
            var mapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                                .SetType(WooProductType.VARIABLE)
                                .SetZaloga(false) // ta je samo osnovni
                                .SetTax(GetTaxMapping())
                                .AddMapping("Barkoda5", "sku")
                                .AddMapping("txtOpis", "name")
                                .AddMapping("PCsPD", "regular_price")
                                .AddCategoryMapping("Barkoda4");
            mapping = await AddAttributes(mapping);
            return (await ArtikelToProductMapping.NullObject()).SetCustom(mapping);
        }

        private async Task<ArtikelToProductMapping> GetVariableProductVariationMapping(IApiClientV2 client, IOutApiClient wooclient) {
            var varmapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                            .SetZaloga(true)
                            .AddMapping("Barkoda3", "sku")
                            .AddMapping("PCsPD", "regular_price");
            varmapping = await AddAttributes(varmapping);
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

        private async Task<BirokratArtikelToWooProductMapping> AddAttributes(BirokratArtikelToWooProductMapping mapping) {
            return await mapping
                .AddAttributeMapping("Barkoda3", new WooAttr() { Name = "Zacasni" });
        }
        #endregion

    }
}

