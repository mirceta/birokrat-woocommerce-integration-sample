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
using core.tools.birokratops;
using core.tools.wooops;
using core.tools.zalogaretriever;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace allintegrations.customers.eigrace
{
    public class EIgraceIntegrationFactory {

        bool debug;
        string datafolder;
        public EIgraceIntegrationFactory(bool debug, string datafolder) {
            this.debug = debug;
            this.datafolder = datafolder;
        }

        public async Task<IIntegration> BuildIntegration(IApiClientV2 client, IOutApiClient wooclient, string biroApiKey, string name) {


            var zaloga = new RetryingZalogaRetriever(new PerPartesZalogaRetriever(client,
               new Dictionary<string, string>() {
                   { "Centralno", "Cent ralno" },
                   { "MP2", "Trgovi na BTC  hala A " } }));

            var integ = new RegularIntegration(client, 
                wooclient, 
                biroApiKey, 
                name, 
                null, 
                new BirokratObvezneNastavitve(new Dictionary<string, string>()), 
                false, 
                new tests.tools.ValidationComponents(new HardcodedCountryMapper("HR"), new NopVatParser(), null, zaloga))
                        .SetBiroToWoo(GetBiroToWoo(client, wooclient, zaloga))
                        //.SetWooToBiro(GetWooToBiro(client, wooclient))
                        .SetPhpConfig(new PhpPluginConfig() {
                            ProductHooks = false,
                            AcceptableAttachmentOrderStatuses = null,
                            AttachmentHook = false,
                            OrderStatusHooks = new List<string>() { }
                        })
                        .SetDatafolder(datafolder);
            integ.Options.Birotowoo_changetracker_dontincludearticlesthatarenotonwebshop = true;
            integ.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(integ, "DEFAULT", null);

            return integ;
        }

        public IBiroToWoo GetBiroToWoo(IApiClientV2 bironext, IOutApiClient wooclient, IZalogaRetriever zaloga) {
            
            var changeHandlers = new List<IBirokratProductChangeHandler>();
            changeHandlers.Add(new PriceChangeHandlerWithSalePriceAdjustment());
            changeHandlers.Add(new ZalogaChangeHandler());

            var simpleProductSyncer = new BiroToWooSimpleProductSyncer(wooclient,
                changeHandlers,
                null,
                BirokratField.Barkoda);
 
           
            var biroArtikelRetriever = new BirokratArtikelRetriever(bironext, zaloga);

            var inner = new SimpleWoocommerceBiroToWoo(bironext,
                wooclient,
                simpleProductSyncer,
                biroArtikelRetriever,
                BirokratField.Barkoda);


            return inner;

        }

        public IWooToBiro GetWooToBiro(IApiClientV2 bironext, IOutApiClient wooclient) {

            var orderflow = new OrderFlow(bironext, null);
            orderflow.AddOrderFlowStage(
                new OrderCondition() { Status = new List<string> { "processing" }, PaymentMethod = null },
                new DocumentInsertionOrderOperationCR(
                    new DocumentInsertion(bironext,
                            BirokratDocumentType.RACUN,
                            new BirokratAttributeIsOriginalOrVariationSku_BirokratPostavkaExtractor(
                                new BirokratPostavkaUtils(false)),
                            new List<IAdditionalOperationOnPostavke>() {
                            new Shipping_PostavkaAddOp(bironext, "01   22 DDV")
                            },
                            null // country mapper!
                    ),
                    BirokratDocumentType.RACUN,
                    new ChangeDocNumOrderOperationCR(bironext, "1-$$$ORDER_NUMBER$$$",
                    !debug ? null : new SaveDocumentOrderOperationCR(bironext, null,
                    datafolder))
                )
            );
            return new OrderFlowProductInserterWooToBiro(orderflow, null);
        }

    }
}
