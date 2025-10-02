using System.Collections.Generic;
using System.Threading.Tasks;
using allintegrations.customers;
using allintegrations.customers.spicasport;
using allintegrations_factories;
using allintegrations_factories.wrappers;
using ApiClient.utils;
using BirokratNext;
using BiroWoocommerceHub;
using BiroWoocommerceHubTests;
using BiroWoocommerceHubTests.tools;
using BiroWooHub.logic.integration;
using core.customers.spicasport;
using core.customers.zgeneric;
using core.logic.common_birokrat;
using core.logic.common_woo;
using core.logic.mapping_biro_to_woo;
using core.logic.mapping_biro_to_woo.change_handlers;
using core.structs;
using core.tools.zalogaretriever;
using tests.tools;
using validator.logic.order_transfer.accessor;

namespace allintegrations_factories.customers.NOVE
{
    public class PolisProjektIntegrationFactory
    {

        bool debug;
        string datafolder;

        VariationalAttributesBuilder variationalAttributesBuilder;

        public PolisProjektIntegrationFactory(bool debug, string datafolder)
        {
            this.debug = debug;
            this.datafolder = datafolder;

            variationalAttributesBuilder = new VariationalAttributesBuilder();
        }

        ValidationComponents validationComponents;
        public async Task<IIntegration> BuildIntegration(bool wootobiro, IApiClientV2 client, IOutApiClient wooclient,
            string biroApiKey, string name, IntegrationIdentifier identifier)
        {
            var zaloga = new RetryingZalogaRetriever(
                new PerPartesZalogaRetriever(client,
                    new Dictionary<string, string>() { 
                        { "MP2", "PE N ES K RANJ" },
                        { "MP7", "TRGOVINA  MARIBOR -GOSPOSKA" },
                        { "MP9", "PE TRGO VINA GA MP PTUJ" },
                        { "MP10", "PE TRGO VINA NE S KOPER" }
                    }));


            validationComponents = new tests.tools.ValidationComponents(
                    new HardcodedCountryMapper(),
                    new VatNumberParser(),
                    variationalAttributesBuilder.GetTestEqualAdditions(),
                    zaloga);

            IWooToBiro wootobiroi = null;
            IBiroToWoo birotowooi = null;
            if (wootobiro)
                wootobiroi = GetWooToBiro(client, wooclient);
            else
                birotowooi = GetBiroToWoo(client, wooclient, zaloga);

            

            var integ = new RegularIntegration(client, wooclient, biroApiKey, name, identifier,
                new BirokratObvezneNastavitve(new Dictionary<string, string>()), true,
                validationComponents)
                            .SetWooToBiro(wootobiroi)
                            .SetBiroToWoo(birotowooi)
                            .SetPhpConfig(new PhpPluginConfig()
                            {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = new List<string>() { },
                                AttachmentHook = true,
                                OrderStatusHooks = new List<string>() { "on-hold", "completed" }
                            })
                            .SetDatafolder(datafolder);

            integ.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(integ, 
                null,
                orderStatuses: new List<string>() { "on-hold", "completed" });
            
            integ.Options.Birotowoo_changetracker_dontincludearticlesthatarenotonwebshop = true;

            //integ.OrderRetriever = new WooNativeRestApiOrderRetriever(wooclient);

            return integ;
        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient)
        {
            var builder = new OrderFlowStageBuilder(debug, client,
                new HardcodedCountryMapper(),
                datafolder);

            builder.SetPostavkeOperations(includePostavkeComments: true,
                percentCoupons: true,
                fixedCartCoupons: true,
                includeShipping: true,
                includeHandlingOproscenaDobava: false);
            builder.SetValidationComponents(validationComponents: validationComponents);
            builder.BuildOrderFlow();

            var docNumTemplate = new JsonIntegrationLoader.utils.OrderAttributeTemplateParser2(
                    "1-$$$ORDER_NUMBER$$$");

            builder.AddStage(
                overrideSklicWithAdditionalNumber: true,
                orderCondition: new OrderCondition()
                {
                    Status = new List<string>() { "on-hold" },
                    PaymentMethod = new List<string>() { "cod", "bacs" }
                },
                doctype: BirokratDocumentType.DOBAVNICA,
                sourceDocType: BirokratDocumentType.UNASSIGNED,
                parser: docNumTemplate);

            builder.AddStage(
                overrideSklicWithAdditionalNumber: true,
                orderCondition: new OrderCondition()
                {
                    Status = new List<string>() { "completed" },
                    PaymentMethod = new List<string>() { "cod", "bacs" }
                },
                doctype: BirokratDocumentType.RACUN,
                sourceDocType: BirokratDocumentType.DOBAVNICA,
                parser: docNumTemplate);

            builder.AddStage(
                overrideSklicWithAdditionalNumber: true,
                orderCondition: new OrderCondition()
                {
                    Status = new List<string>() { "completed" },
                    PaymentMethod = new List<string>() { "cod", "bacs" },
                    NegatePaymentMethod = true
                },
                doctype: BirokratDocumentType.RACUN,
                sourceDocType: BirokratDocumentType.UNASSIGNED,
                parser: docNumTemplate,
                fiscallize: true);

            var orderflow = builder.GetOrderFlow();
            return new OrderFlowProductInserterWooToBiro(orderflow, null);
        }

        public IBiroToWoo GetBiroToWoo(IApiClientV2 client, IOutApiClient wooclient, IZalogaRetriever zaloga)
        {
            var changeHandlers = new List<IBirokratProductChangeHandler>();
            changeHandlers.Add(new PriceChangeHandlerWithSalePriceAdjustment());
            changeHandlers.Add(new ZalogaChangeHandler());


            // TREBA SEM SE DODAT KAR JE PRAV!
            variationalAttributesBuilder
                .AddVariationAttribute("1___________________VELIKOST", new WooAttr() { Name = "Velikost" });


            var biroArtikelRetriever = new BirokratArtikelRetriever(client, zaloga);


            IBiroToWoo inner = new VariableBiroToWooBuilder(client, wooclient, changeHandlers, biroArtikelRetriever,
                BirokratField.SifraArtikla,
                BirokratField.Barkoda5,
                variationalAttributesBuilder,
                zaloga: true,
                addOnFailToUpdate: false).Build();

            return inner;

        }
    }
}


/*
         woo: barva, brand, velikost

        // mora se narest v birokratu dodatne atribute

        zaloga:4 skladisca, sestejes celo zalogo in sinhroniziras.
            PE, Trgovina Gamb ptuj, Trgovina maribor - gosposka, PENes kranj, PE trgovina NES Koper.



        narocila:

        proc + vred kuponi
        fizicnim osebam
        opisi v posamezne postavke
        uvodni tekst: Na narocilo damo blabla

        Se postopki pri izdajanju naročila delijo glede na tip plačila?
        PREDRACUN -> on-hold = dobavnica, completed = racun
        KARTICA -> completed = racun
        Kdaj se racun fiskalizira? AVTOMATSKO. V FAZI ZAKLJUCENO.

        ni razlicnih drzav

        ne more it zaloga v minus
         */