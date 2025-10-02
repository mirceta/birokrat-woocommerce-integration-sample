using allintegrations.customers;
using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub;
using BiroWoocommerceHub;
using BiroWoocommerceHub.logic;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
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
using core.structs;
using core.tools.zalogaretriever;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using tests.tools;

namespace allintegrations_factories.customers.partypek
{
    public class PartypekIntegrationFactory {
        bool debug;
        string datafolder;

        public PartypekIntegrationFactory(bool debug, string datafolder) {
            this.debug = debug;
            this.datafolder = datafolder;
        }

        public async Task<IIntegration> BuildIntegration(bool wootobiro, 
            IApiClientV2 client,
            IOutApiClient wooclient, 
            string biroApiKey, 
            string name, 
            IntegrationIdentifier identifier) {

            IBiroToWoo birotowooi = null;
            IWooToBiro wootobiroi = null;


            var zaloga = new RetryingZalogaRetriever(new PerPartesZalogaRetriever(client,
                new Dictionary<string, string>() {
                   { "Centralno", "Cent ralno" } }));

            if (wootobiro)
                wootobiroi = GetWooToBiro(client, wooclient);
            else
                birotowooi = GetBiroToWoo(client, wooclient, zaloga);

            var integ = new RegularIntegration(client, wooclient, biroApiKey, name, identifier,
                new BirokratObvezneNastavitve(new Dictionary<string, string>() {
                                            { "DobavnicaRazknjizuje", "true" }
                                        }),
                includeValidator: true,
                new ValidationComponents(
                                new WooStaticCountryMapper(new Dictionary<string, string>() {
                                    { "SI", "SLO"},
                                    { "HR", "HR"}
                                }),
                                new VatNumberParser(),
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

            integ.Options.Birotowoo_changetracker_dontincludearticlesthatarenotonwebshop = true;


            integ.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(integ, "DEFAULT", new List<string>() { "processing" });

            return integ;
        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient) {

            var mapper = new WooStaticCountryMapper(new Dictionary<string, string>() {
                    { "SI", "SLO"},
                    { "HR", "HR"}
                });

            // partner vnos TODO
            var vatIdParser = new VatNumberParser();
            var statusZavMapper = new EstradaStatusPartnerjaMapper(vatIdParser);

            var partnerMapper = new SwitchOnDavcnaPartnerInserter(client, vatIdParser,
                new PartnerWooToBiroMapper1(mapper, statusZavMapper, statusZavMapper),
                povoziVseAtribute: true);

            // order flow
            var orderflow = new OrderFlow(client, partnerMapper);
            orderflow.AddOrderFlowStage(
                new OrderCondition() { Status = new List<string>() { "processing", "on-hold" }, PaymentMethod = null },
                new DocumentInsertionOrderOperationCR(
                    new DocumentInsertion(client,
                            BirokratDocumentType.DOBAVNICA,
                            // s tem da damo BirokratPostavkaUtils.total_price_instead_of_subtotal na true,
                            // poskrbimo za procentni kupon - ne bo vidnih 10% popusta na dobavnici, vendar
                            // se znebimo OneCentExceptionov (v resnici lahko do par centov!!!)
                            new BirokratAttributeIsOriginalOrVariationSku_BirokratPostavkaExtractor(new BirokratPostavkaUtils(true)),
                            new List<IAdditionalOperationOnPostavke>() {
                                //new CouponPercent_PostavkeAddOp(),
                                new Shipping_PostavkaAddOp(client, "4    22 DDV osnovna stopnja            Storitev")
                            },
                            mapper // country mapper!
                    ).SetAdditionalParams((x) => new OrderAdditionalParams() {
                        CountryMapper = mapper,
                        AdditionalNumber = x.Data.Number,
                        ExternalUniqueIdentifier = x.Data.Number,
                        SourceDocumentType = BirokratDocumentType.UNASSIGNED,
                        SourceDocumentNumberExtractor = null
                    }),
                    BirokratDocumentType.DOBAVNICA,
                    new ChangeDocNumOrderOperationCR(client, "1-$$$ORDER_NUMBER$$$",
                        !debug ? null : new SaveDocumentOrderOperationCR(client,
                            null, //new ObvestiloOPotekliZalogiOrderOperationCR(client, wooclient, null),
                            datafolder
                        )
                    )
                )
            );
            return new OrderFlowProductInserterWooToBiro(orderflow, null);
        }

        public IBiroToWoo GetBiroToWoo(IApiClientV2 bironext, 
            IOutApiClient wooclient,
            IZalogaRetriever zaloga) {
            var changeHandlers = new List<IBirokratProductChangeHandler>();
            changeHandlers.Add(new PriceChangeHandlerWithSalePriceAdjustment("."));
            changeHandlers.Add(new ZalogaChangeHandler());

            var simpleProductSyncer = new BiroToWooSimpleProductSyncer(wooclient,
                changeHandlers,
                null,
                BirokratField.SifraArtikla);

            var biroArtikelRetriever = new BirokratArtikelRetriever(bironext, zaloga);

            var inner = new SimpleWoocommerceBiroToWoo(bironext, wooclient, simpleProductSyncer, biroArtikelRetriever, BirokratField.SifraArtikla);

            return inner;
        }
    }
}
