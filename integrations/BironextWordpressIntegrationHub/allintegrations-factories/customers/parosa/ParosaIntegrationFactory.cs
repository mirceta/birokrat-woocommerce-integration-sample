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

namespace allintegrations_factories.customers.parosa
{
    public class ParosaIntegrationFactory {
        bool debug;
        string datafolder;

        public ParosaIntegrationFactory(bool debug, string datafolder) {
            this.debug = debug;
            this.datafolder = datafolder;
        }

        public async Task<IIntegration> BuildIntegration(
            IApiClientV2 client,
            IOutApiClient wooclient,
            string biroApiKey,
            string name,
            IntegrationIdentifier identifier) {

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
                                null
                ))
                            .SetWooToBiro(GetWooToBiro(client, wooclient))
                            .SetPhpConfig(new PhpPluginConfig() {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = null,
                                AttachmentHook = false,
                                OrderStatusHooks = new List<string>() { "processing" }
                            })
                            .SetDatafolder(datafolder);

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


            var postavkaExtractor = new BirokratAttributeIsOriginalSku_BirokratPostavkaExtractor(new BirokratPostavkaUtils(false));
            var additionPostavkeOps = new List<IAdditionalOperationOnPostavke>() {
                                new CommentAddVarAttrs_PostavkaAddOp(true),
                                new Shipping_PostavkaAddOp(client, "6     0 DDV oproščen promet            Storitev")
                            };

            // order flow
            var orderflow = new OrderFlow(client, partnerMapper);
            orderflow.AddOrderFlowStage(
                new OrderCondition() { 
                    Status = new List<string>() { "on-hold" }, 
                    PaymentMethod = new List<string>() { "bacs" } },
                new DocumentInsertionOrderOperationCR(
                    new DocumentInsertion(client,
                            BirokratDocumentType.PREDRACUN,
                            postavkaExtractor,
                            additionPostavkeOps,
                            null // country mapper!
                    ).SetAdditionalParams((x) => new OrderAdditionalParams() {
                        CountryMapper = null,
                        AdditionalNumber = x.Data.Number,
                        ExternalUniqueIdentifier = x.Data.Number,
                        SourceDocumentType = BirokratDocumentType.UNASSIGNED,
                        SourceDocumentNumberExtractor = null
                    }),
                    BirokratDocumentType.PREDRACUN,
                    new ChangeDocNumOrderOperationCR(client, "1-$$$ORDER_NUMBER$$$",
                        !debug ? null : new SaveDocumentOrderOperationCR(client,
                            null, //new ObvestiloOPotekliZalogiOrderOperationCR(client, wooclient, null),
                            datafolder
                        )
                    )
                )
            );

            orderflow.AddOrderFlowStage(
                new OrderCondition() { 
                    Status = new List<string>() { "processing" }, 
                    PaymentMethod = new List<string>() { "stripe" } },
                new DocumentInsertionOrderOperationCR(
                    new DocumentInsertion(client,
                            BirokratDocumentType.RACUN,
                            postavkaExtractor,
                            additionPostavkeOps,
                            null // country mapper!
                    ).SetAdditionalParams((x) => new OrderAdditionalParams() {
                        CountryMapper = null,
                        AdditionalNumber = x.Data.Number,
                        ExternalUniqueIdentifier = x.Data.Number,
                        SourceDocumentType = BirokratDocumentType.UNASSIGNED,
                        SourceDocumentNumberExtractor = null
                    }),
                    BirokratDocumentType.RACUN,
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
    }
}
