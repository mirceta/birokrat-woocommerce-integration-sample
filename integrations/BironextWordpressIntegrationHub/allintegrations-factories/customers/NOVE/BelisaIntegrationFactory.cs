using allintegrations.customers;
using allintegrations_factories.customers.estrada;
using BirokratNext;
using BiroWoocommerceHub.logic;
using BiroWoocommerceHub;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.customers.poledancerka.mappers;
using core.customers.poledancerka;
using core.customers.spicasport;
using core.customers.zgeneric;
using core.logic.common_birokrat;
using core.logic.common_woo;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.logic.mapping_woo_to_biro.order_operations.pl;
using core.logic.mapping_woo_to_biro.order_operations;
using core.logic.mapping_woo_to_biro.product_ops;
using core.logic.mapping_woo_to_biro;
using core.structs;
using core.tools.zalogaretriever;
using System;
using System.Collections.Generic;
using System.Text;
using tests.tools;
using core.tools.wooops;
using BironextWordpressIntegrationHub;
using ApiClient.utils;
using System.Threading.Tasks;

namespace allintegrations_factories.customers.NOVE
{
    public class BelisaIntegrationFactory
    {

        bool debug;
        string datafolder;
        public BelisaIntegrationFactory(bool debug, string datafolder)
        {
            this.debug = debug;
            this.datafolder = datafolder;
        }



        public async Task<IIntegration> BuildIntegration(IApiClientV2 client, IOutApiClient wooclient, string biroApiKey, string name = "ESTRADAOSNOVNA")
        {

            var tmp = new RegularIntegration(client, wooclient, biroApiKey,
                            name, new IntegrationIdentifier() { },
                            new BirokratObvezneNastavitve(new Dictionary<string, string>() {
                                { "eTrgovanjeCenaSPDFix", "true"},
                                { "eTrgovanjeDrzavaPDFix", "USA  Amerika"}
                            }),
                            includeValidator: true,
                            new ValidationComponents(
                                new HardcodedCountryMapper(),
                                new EstradaVatIdParser(client),
                                null,
                                new RetryingZalogaRetriever(null)
                            )
                            )
                            .SetWooToBiro(GetWooToBiro(client, wooclient))
                            .SetBiroToWoo(null)
                            .SetPhpConfig(new PhpPluginConfig()
                            {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = null,
                                AttachmentHook = false,
                                OrderStatusHooks = new List<string>() { "completed" }
                            })
                            .SetDatafolder(datafolder);

            tmp.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(tmp, "DEFAULT", new List<string>() { "completed" });

            tmp.TestingConfiguration.WooToBiro.EndDate = new DateTime(2023, 2, 1);

            return tmp;
        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient)
        {
            var mapper = new HardcodedCountryMapper();
            var countryMapper = mapper;



            var simpleMapper = new ClassicSimpleProductMapper(new BirokratPostavkaUtils(false), client, false);
            IWooToBiroProductMapper variableMapper = null;
            variableMapper = new ClassicVariableProductMapper(new BirokratPostavkaUtils(false), client, false, 2, new RegularSkuToSearch());


            var productInserter = new PoledancerkaWooToBiroProductInserter(wooclient, simpleMapper, variableMapper, null);

            List<IWooToBiroProductMapper> lst = new List<IWooToBiroProductMapper>();
            lst.Add(simpleMapper);
            lst.Add(variableMapper);
            var compositeMapper = new CompositeWooItem_BirokratPostavkaExtractor(client, lst, true);

            // vnos kupcev

            var vatIdParser = new VatNumberParser();
            var statusZavMapper = new EstradaStatusPartnerjaMapper(vatIdParser);
            var partnerMapper = new SwitchOnDavcnaPartnerInserter(client, vatIdParser,
                new PartnerWooToBiroMapper1(mapper, statusZavMapper, statusZavMapper),
                povoziVseAtribute: true);

            var orderflow = new OrderFlow(client, partnerMapper);
            orderflow = AddRegularCase(orderflow, client, countryMapper, compositeMapper);

            return new OrderFlowProductInserterWooToBiro(orderflow, productInserter);
        }

        private OrderFlow AddRegularCase(OrderFlow orderflow, IApiClientV2 client, ICountryMapper countryMapper, CompositeWooItem_BirokratPostavkaExtractor compositeMapper)
        {

            var multiplyIfOproscenaDobava = new PriceMultiplierByVAT_ForOproscenaDobava_PostavkaAddOp(new BirokratPostavkaUtils(false), countryMapper,
                 new VatExemptX());
            var multiplyIfETrgovanje = new PriceMultiplierByVATRatio_PostavkaAddOp(new BirokratPostavkaUtils(false), countryMapper,
                 (order) => !GWooOps.IsVatExempt(order));


            var multipliers = new List<IAdditionalOperationOnPostavke>() { multiplyIfOproscenaDobava, multiplyIfETrgovanje };
            var addops = new List<IAdditionalOperationOnPostavke>() {
                                multiplyIfOproscenaDobava,
                                multiplyIfETrgovanje,
                                new CouponPercent_PostavkeAddOp(),
                                new CouponFixedCart_PostavkeAddOp(client, multipliers),
                                new Shipping_PostavkaAddOp(client, "4    22 DDV osnovna stopnja            Storitev",
                                    multipliers)
            };

            orderflow.AddOrderFlowStage(
                        new OrderCondition() { Status = new List<string> { "completed" }, PaymentMethod = null },
                        new DocumentInsertionOrderOperationCR(
                            new DocumentInsertion(client,
                                    BirokratDocumentType.RACUN,
                                    compositeMapper,
                                    addops,
                                    countryMapper // country mapper!
                            ).SetAdditionalParams((x) => new OrderAdditionalParams()
                            {
                                CountryMapper = countryMapper,
                                AdditionalNumber = x.Data.Id + "",
                                ExternalUniqueIdentifier = x.Data.Number,
                                SourceDocumentType = BirokratDocumentType.UNASSIGNED,
                                SourceDocumentNumberExtractor = null
                            }),
                            BirokratDocumentType.RACUN,
                            new DocumentParametersModifierOrderOperationCR(client, EstradaOrderParams.Get(), countryMapper,
                            !debug ? null : new SaveDocumentOrderOperationCR(client,
                            null,
                            datafolder))
                        )
                    );

            return orderflow;
        }
    }
}
