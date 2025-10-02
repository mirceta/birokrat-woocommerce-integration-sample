using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using allintegrations.customers;
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
using core.logic.mapping_woo_to_biro.product_ops;
using core.structs;
using core.tools.attributemapper;
using core.tools.birokratops;
using core.tools.wooops;
using core.tools.zalogaretriever;
using gui_inferable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace allintegrations_factories.customers.hisavizij
{

    class Neki : IInferable
    {
        public Dictionary<string, object> Infer(Dictionary<string, object> state)
        {
            throw new NotImplementedException();
        }
    }

    public class HisaVizijIntegrationFactory
    {
        bool debug;
        string datafolder;

        public HisaVizijIntegrationFactory(bool debug, string datafolder) {
            this.debug = debug;
            this.datafolder = datafolder;
        }

        public async Task<IIntegration> BuildIntegration(bool wootobiro, IApiClientV2 client, IOutApiClient wooclient, string biroApiKey, string name, IntegrationIdentifier identifier) {

            var zaloga = new RetryingZalogaRetriever(new PerPartesZalogaRetriever(client,
                new Dictionary<string, string>() {
                   { "MP1", "SMILE" },
                   { "MP3", "Kons igna cija" } },
                popraviZaSestavljeneArtikle: true));

            IWooToBiro wootobiroi = null;
            IBiroToWoo birotowooi = null;
            if (wootobiro)
                wootobiroi = GetWooToBiro(client, wooclient);
            else
                birotowooi = await GetBiroToWoo(client, wooclient, zaloga);

            var integ = new RegularIntegration(client, wooclient, biroApiKey, name, identifier,
                new BirokratObvezneNastavitve(new Dictionary<string, string>() {
                                            { "DobavnicaRazknjizuje", "true" }
                                        }),
                includeValidator: true,
                new tests.tools.ValidationComponents(new WooStaticCountryMapper(new Dictionary<string, string>() {
                    { "SI", "SLO"},
                    { "HR", "CRO"}
                }, defaultCountry: "SLO"), 
                new EstradaVatIdParser(client),
                null,
                zaloga
                ))
                            .SetWooToBiro(wootobiroi)
                            .SetBiroToWoo(birotowooi)
                            .SetPhpConfig(new PhpPluginConfig() {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = null,
                                AttachmentHook = false,
                                OrderStatusHooks = new List<string>() { "processing", "on-hold" }
                            })
                            .SetDatafolder(datafolder);

            integ.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(integ, "DEFAULT", new List<string>() { "processing" });
            integ.TestingConfiguration.BiroToWoo.allowMultipleProductsWithSameSkuOnWebshop = true;
            return integ;
        }

        public async Task<IBiroToWoo> GetBiroToWoo(IApiClientV2 bironext, IOutApiClient wooclient, IZalogaRetriever zaloga) {
            var changeHandlers = new List<IBirokratProductChangeHandler>();
            changeHandlers.Add(new PriceChangeHandlerWithSalePriceAdjustment());
            changeHandlers.Add(new ZalogaChangeHandler());

            var simpleProductSyncer = new BiroToWooSimpleProductSyncer(wooclient,
                changeHandlers,
                await GetSimpleProductMapping(bironext, wooclient),
                BirokratField.SifraArtikla,
                addOnFailToUpdate: true);

            var variableProductSyncer = new BiroToWooVariableProductSyncer(wooclient,
                changeHandlers,
                await GetVariableProductBaseMapping(bironext, wooclient),
                await GetVariableProductVariationMapping(bironext, wooclient),
                BirokratField.Barkoda5,
                BirokratField.SifraArtikla,
                addOnFailToUpdate: true,
                requireBaseSkuAttrPrefixOfVariationSkuAttr: false);

            var biroArtikelRetriever = new BirokratArtikelRetriever(bironext, zaloga);



            IBiroToWoo inner = new RegularWoocommerceBiroToWoo(bironext,
                wooclient,
                simpleProductSyncer,
                variableProductSyncer,
                biroArtikelRetriever,
                BirokratField.Barkoda5,
                BirokratField.SifraArtikla);

            return inner;
        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient) {

            // TODO PARTNER: HardcodedCountryMapper is not enough - they have custs from all over the world.
            var mapper = new WooStaticCountryMapper(new Dictionary<string, string>() {
                    { "SI", "SLO"},
                    { "HR", "CRO"}
                }, defaultCountry: "SLO");
            var countryMapper = mapper;
            var vatIdParser = new EstradaVatIdParser(client);
            var statusZavMapper = new EstradaStatusPartnerjaMapper(vatIdParser);
            var partnerMapper = new SwitchOnDavcnaPartnerInserter(client, vatIdParser,
                new PartnerWooToBiroMapper1(countryMapper, statusZavMapper, statusZavMapper),
                povoziVseAtribute: true);

            // order flow
            var addops = new List<IAdditionalOperationOnPostavke>() {
                                new CommentAddOriginProductSku_PostavkaAddOp(false),
                                new CouponPercent_PostavkeAddOp(),
                                new Shipping_PostavkaAddOp(client, "6     0 DDV oproščen promet            Storitev"),
                                new ConditionalAddExpense_PostavkaAddOp(client, new PaymentMethod_OrderAddOpCondition("cod"), 
                                    "Strošek", 
                                    "1.00",
                                    "6     0 DDV oproščen promet            Storitev"),
                                new PriceMultiplier_PostavkaAddOp(new BirokratPostavkaUtils(false), 1.0 / 7.5345, new Currency_OrderAddOpCondition("HRK"))
            };


            var orderflow = new OrderFlow(client, partnerMapper);


            

            orderflow.AddOrderFlowStage(
                        new OrderCondition() { Status = new List<string> { "processing", "on-hold" }, PaymentMethod = null },
                        new DocumentInsertionOrderOperationCR(
                            new DocumentInsertion(client,
                                    BirokratDocumentType.DOBAVNICA,
                                    new BirokratAttributeIsOriginalOrVariationSku_BirokratPostavkaExtractor(new BirokratPostavkaUtils(false)),
                                    addops,
                                    countryMapper // country mapper!
                            ).SetAdditionalParams((x) => new OrderAdditionalParams() {
                                CountryMapper = countryMapper,
                                AdditionalNumber = x.Data.Id + "",
                                ExternalUniqueIdentifier = x.Data.Number,
                                SourceDocumentType = BirokratDocumentType.UNASSIGNED,
                                SourceDocumentNumberExtractor = null
                            }),
                            BirokratDocumentType.DOBAVNICA,
                            GetAdditionalOrderOps(client, mapper, 
                            !debug ? null : new SaveDocumentOrderOperationCR(client,
                            null,
                            datafolder)))
                        );
                    

            return new OrderFlowProductInserterWooToBiro(orderflow, null);
        }

        DocumentParametersModifierOrderOperationCR GetAdditionalOrderOps(IApiClientV2 client, ICountryMapper mapper, IOrderOperationCR next) {
            List<DocumentParameterCommand> operations = new List<DocumentParameterCommand>();

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("DatumValute")
                    .SetCondition(new Tautology())
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new DaysFromNow(7))
                    .Build());
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("DatumOdpreme")
                    .SetCondition(new Tautology())
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Const(""))
                    .Build());

            return new DocumentParametersModifierOrderOperationCR(client, operations, mapper, next);
        }

        
        #region [birotowoo]
        private async Task<ArtikelToProductMapping> GetSimpleProductMapping(IApiClientV2 client, IOutApiClient wooclient) {
            var mapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                            .SetType(WooProductType.SIMPLE)
                            .SetZaloga(true)
                            .SetTax(GetTaxMapping())
                            .AddMapping("txtSifraArtikla", "sku")
                            .AddMapping("txtOpis", "name")
                            .AddMapping("PCsPD", "regular_price");
            mapping = await AddAttributes(mapping);
            return (await ArtikelToProductMapping.NullObject()).SetCustom(mapping);
        }

        private async Task<ArtikelToProductMapping> GetVariableProductBaseMapping(IApiClientV2 client, IOutApiClient wooclient) {
            var mapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                                .SetType(WooProductType.VARIABLE)
                                .SetZaloga(false) // ta je samo osnovni
                                .SetTax(GetTaxMapping())
                                .AddMapping("Barkoda5", "sku")
                                .AddMapping("txtOpis", "name")
                                .AddMapping("PCsPD", "regular_price");
            mapping = await AddAttributes(mapping);
            return (await ArtikelToProductMapping.NullObject()).SetCustom(mapping);
        }

        private async Task<ArtikelToProductMapping> GetVariableProductVariationMapping(IApiClientV2 client, IOutApiClient wooclient) {
            var varmapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                            .SetZaloga(true)
                            .AddMapping("txtSifraArtikla", "sku")
                            .AddMapping("PCsPD", "regular_price");
            varmapping = await AddAttributes(varmapping);
            return (await ArtikelToProductMapping.NullObject()).SetCustom(varmapping);
        }

        private BiroTaxToWooTax GetTaxMapping() {
            // TOREJ OD DAVKOV ZAENKRAT PODPIRAMO SAMO TO!!! - VI SI JIH USTVARITE SAMI, SAMO POVEJTE MI KAKO SE BIROKRAT ATRIBUTI PRESLIKAJO!
            BiroTaxToWooTax tax = new BiroTaxToWooTax("SifraDavka", "tax_class");

            tax.AddMapping("1    22 DDV osnovna stopnja", "standard");
            tax.AddMapping("10    5 DDV znižana stopnja 5 %", "zero-rate");
            tax.AddMapping("2   9.5 DDV znižana stopnja", "reduced-rate");
            tax.AddMapping("3     0 DDV oproščen promet", "zero-rate");
            tax.AddMapping("4    22 DDV osnovna stopnja            Storitev", "standard");
            tax.AddMapping("5   9.5 DDV znižana stopnja            Storitev", "reduced-rate");
            tax.AddMapping("6     0 DDV oproščen promet            Storitev", "zero-rate");
            tax.AddMapping("7    22 Prejemniki plačniki DDV        Storitev", "standard");
            tax.AddMapping("8   9.5 Prejemniki plačniki DDV        Storitev", "reduced-rate");
            tax.AddMapping("9     8 Pavšalno nadomestilo", "reduced-rate");
            return tax;
        }


        private async Task<BirokratArtikelToWooProductMapping> AddAttributes(BirokratArtikelToWooProductMapping mapping) {
            return await mapping
                .AddAttributeMapping("01__________________VEL", new WooAttr("Velikost", true, true));
        }
        #endregion
    }
}
