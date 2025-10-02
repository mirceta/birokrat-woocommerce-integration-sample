using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.logic;
using BiroWoocommerceHub.structs_wc_to_biro;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.customers.poledancerka;
using core.customers.poledancerka.mappers;
using core.customers.spicasport;
using core.customers.zgeneric;
using core.logic.common_birokrat;
using core.logic.common_woo;
using core.logic.mapping_biro_to_woo;
using core.logic.mapping_biro_to_woo.change_handlers;
using core.logic.mapping_woo_to_biro;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using core.logic.mapping_woo_to_biro.order_operations;
using core.logic.mapping_woo_to_biro.product_ops;
using core.structs;
using core.tools.birokratops;
using core.tools.wooops;
using core.tools.zalogaretriever;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace allintegrations.customers.poledancerka
{
    public class NasaTestnaIntegrationFactory {

        public static IIntegration BuildIntegration(IApiClientV2 client, IOutApiClient wooclient, string biroApiKey, bool isb2b, string name, IntegrationIdentifier identifier) {

            return new RegularIntegration(client, wooclient, biroApiKey, name, null, 
                new core.logic.common_birokrat.BirokratObvezneNastavitve(new Dictionary<string, string>()), false, null)
                            .SetWooToBiro(GetWooToBiro(client, wooclient, isb2b))
                            .SetBiroToWoo(GetBiroToWoo(client, wooclient))
                            .SetPhpConfig(new PhpPluginConfig() {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = new List<string>() { "completed" },
                                AttachmentHook = true,
                                OrderStatusHooks = new List<string>() { "processing" }
                            });
        }

        public static IBiroToWoo GetBiroToWoo(IApiClientV2 bironext, IOutApiClient wooclient) {

            var changeHandlers = new List<IBirokratProductChangeHandler>();
            changeHandlers.Add(new PriceChangeHandlerWithSalePriceAdjustment());
            changeHandlers.Add(new ZalogaChangeHandler());

            var simpleProductSyncer = new BiroToWooSimpleProductSyncer(wooclient,
                changeHandlers,
                null,
                BirokratField.SifraArtikla);

            var zaloga = new RetryingZalogaRetriever(new PerPartesZalogaRetriever(bironext,
                new Dictionary<string, string>() {
                   { "Centralno", "Cent ralno" }
                }));
            var biroArtikelRetriever = new BirokratArtikelRetriever(bironext, zaloga);

            var inner = new SimpleWoocommerceBiroToWoo(bironext,
                wooclient,
                simpleProductSyncer,
                biroArtikelRetriever,
                BirokratField.SifraArtikla);

            return inner;
        }

        public static IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient, bool isb2b) {

            var simpleMapper = new ClassicSimpleProductMapper(new BirokratPostavkaUtils(false), client, false);
            IWooToBiroProductMapper variableMapper = null;
            IWooToBiroProductMapper complexMapper = null;
            
            variableMapper = new ClassicVariableProductMapper(new BirokratPostavkaUtils(false), client, false, 2, new PoledancerkaSkuToSearch());
            complexMapper = new DancerkaComplexProductMapper(new BirokratPostavkaUtils(false), client, false, 2);
            

            var productInserter = new PoledancerkaWooToBiroProductInserter(wooclient, simpleMapper, variableMapper, complexMapper);

            List<IWooToBiroProductMapper> lst = new List<IWooToBiroProductMapper>();
            lst.Add(simpleMapper);
            lst.Add(variableMapper);
            lst.Add(complexMapper);
            var compositeMapper = new CompositeWooItem_BirokratPostavkaExtractor(client, lst, !isb2b);



            var statusZavMapper = new B2CStatusPartnerjaMapper();

            var partnerInsert = new ClassicPartnerInserter(client,
                new PartnerWooToBiroMapper1(new HardcodedCountryMapper(), statusZavMapper, statusZavMapper),
                new VatNumberParser());

            var orderflow = new OrderFlow(client, partnerInsert);
            orderflow = AddRegularCase(orderflow, client, null, compositeMapper);
            
            return new OrderFlowProductInserterWooToBiro(orderflow, productInserter);
        }

        private static OrderFlow AddRegularCase(OrderFlow orderflow, IApiClientV2 client, WooToBiroCountryMapper countryMapper, CompositeWooItem_BirokratPostavkaExtractor compositeMapper) {
           
            orderflow.AddOrderFlowStage(
                        new OrderCondition() { Status = new List<string> { "processing" }, PaymentMethod = null },
                        new DocumentInsertionOrderOperationCR(
                            new DocumentInsertion(client,
                                    BirokratDocumentType.RACUN,
                                    compositeMapper,
                                    new List<IAdditionalOperationOnPostavke>() {
                                new CommentAddVarAttrs_PostavkaAddOp(true),
                                new CouponPercent_PostavkeAddOp(),
                                new CouponFixedCart_PostavkeAddOp(client),
                                new Shipping_PostavkaAddOp(client)
                                    },
                                    countryMapper // country mapper!
                            ).SetAdditionalParams((x) => new OrderAdditionalParams() {
                                CountryMapper = countryMapper,
                                AdditionalNumber = x.Data.Id + "",
                                ExternalUniqueIdentifier = x.Data.Number,
                                SourceDocumentType = BirokratDocumentType.UNASSIGNED,
                                SourceDocumentNumberExtractor = null
                            }),
                            BirokratDocumentType.RACUN,
                            new DancerkaOrderModificationOrderOperationCR(client, countryMapper,null)
                            //new SaveDocumentOrderOperationCR(client,
                            //null,
                            //@"C:\Users\km\Desktop\playground\bironext-woocommerce-integration\BironextWordpressIntegrationHub\BiroWoocommerceHubTests\jsons\orders\dancerka\dancerkaproofpdfs"))
                        )
                    );

            orderflow.AddAttachmentFlowStage(
                new OrderCondition() { Status = new List<string> { "completed" }, PaymentMethod = null },
                new DocumentNumberGetter_ByOrderAttributeTemplate(client, "$$$ORDER_ID$$$", BirokratDocumentType.RACUN,
                     new BiroDocumentPdfGetter(client, null)));

            return orderflow;
        }

    }
}
