using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using allintegrations.customers;
using allintegrations.customers.spicasport;
using allintegrations_factories;
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
using core.structs;
using core.tools.zalogaretriever;
using validator.logic.order_transfer.accessor;

namespace allintegrations_factories.customers.NOVE
{

    public partial class MenhartIntegrationFactory
    {

        bool debug;
        string datafolder;

        VariationalAttributesBuilder variationalAttributesBuilder;

        public MenhartIntegrationFactory(bool debug, string datafolder)
        {
            this.debug = debug;
            this.datafolder = datafolder;

            variationalAttributesBuilder = new VariationalAttributesBuilder();
        }

        public async Task<IIntegration> BuildIntegration(bool wootobiro, IApiClientV2 client, IOutApiClient wooclient,
            string biroApiKey, string name, IntegrationIdentifier identifier)
        {
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
                    variationalAttributesBuilder.GetTestEqualAdditions(),
                    zaloga))
                            .SetWooToBiro(wootobiroi)
                            .SetBiroToWoo(birotowooi)
                            .SetPhpConfig(new PhpPluginConfig()
                            {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = new List<string>() { },
                                AttachmentHook = false,
                                OrderStatusHooks = new List<string>() { "on-hold", "processing" }
                            })
                            .SetDatafolder(datafolder);

            integ.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(integ, null, new List<string>() { "on-hold", "processing" });


            integ.Options.Birotowoo_changetracker_dontincludearticlesthatarenotonwebshop = true;
            integ.Options.Birotowoo_dont_include_validator = true;

            return integ;
        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient)
        {
            var builder = new OrderFlowStageBuilder(debug, client,
                new HardcodedCountryMapper(),
                datafolder);

            builder.SetPostavkeOperations(includePostavkeComments: false,
                percentCoupons: true,
                fixedCartCoupons: true,
                includeShipping: true,
                includeHandlingOproscenaDobava: false);
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
                    Status = new List<string>() { "processing" },
                    PaymentMethod = new List<string>() { "cod", "bacs" }
                },
                doctype: BirokratDocumentType.RACUN,
                sourceDocType: BirokratDocumentType.DOBAVNICA,
                parser: docNumTemplate);

            builder.AddStage(
                overrideSklicWithAdditionalNumber: true,
                orderCondition: new OrderCondition()
                {
                    Status = new List<string>() { "processing" },
                    PaymentMethod = new List<string>() { "cod", "bacs" },
                    NegatePaymentMethod = true
                },
                doctype: BirokratDocumentType.RACUN,
                sourceDocType: BirokratDocumentType.UNASSIGNED,
                parser: docNumTemplate,
                fiscallize: false);

            var orderflow = builder.GetOrderFlow();


            return new OrderFlowProductInserterWooToBiro(orderflow, null);
        }

        public async Task<IBiroToWoo> GetBiroToWoo(IApiClientV2 client, IOutApiClient wooclient, IZalogaRetriever zaloga)
        {
            var changeHandlers = new List<IBirokratProductChangeHandler>();
            changeHandlers.Add(new PriceChangeHandlerWithSalePriceAdjustment("."));
            changeHandlers.Add(new ZalogaChangeHandler());


            variationalAttributesBuilder
                .AddVariationAttribute("ve__________________podrobnosti", new WooAttr() { Name = "Velikost" })
                .AddVariationAttribute("ba__________________Podrobnosti", new WooAttr() { Name = "Barva" });


            var biroArtikelRetriever = new BirokratArtikelRetriever(client, zaloga);


            IBiroToWoo inner = (await new VariableBiroToWooBuilder(
                client,
                wooclient,
                changeHandlers,
                biroArtikelRetriever,
                BirokratField.SifraArtikla,
                BirokratField.Barkoda4,
                variationalAttributesBuilder,
                zaloga: true,
                addOnFailToUpdate: false).Setup()).Build();
            
            return inner;

        }
    }
}
