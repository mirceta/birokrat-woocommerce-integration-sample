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
using BiroWooHub.logic.integration;
using core.customers.poledancerka;
using core.customers.poledancerka.mappers;
using core.customers.spicasport;
using core.customers.zgeneric;
using core.logic.common_birokrat;
using core.logic.common_woo;
using core.logic.mapping_woo_to_biro;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using core.logic.mapping_woo_to_biro.order_operations;
using core.logic.mapping_woo_to_biro.order_operations.pl;
using core.logic.mapping_woo_to_biro.orderflow.order_operations;
using core.logic.mapping_woo_to_biro.product_ops;
using core.structs;
using core.tools.birokratops;
using core.tools.wooops;
using core.tools.zalogaretriever;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using tests.tools;

namespace allintegrations_factories.customers.estrada
{
    public class EstradaIntegrationFactory {

        bool debug;
        string datafolder;
        public EstradaIntegrationFactory(bool debug, string datafolder) {
            this.debug = debug;
            this.datafolder = datafolder;
        }

        public async Task<IIntegration> BuildIntegration(IApiClientV2 client, IOutApiClient wooclient, string biroApiKey, string name = "ESTRADAOSNOVNA") {

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
                            .SetPhpConfig(new PhpPluginConfig() {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = null,
                                AttachmentHook = false,
                                OrderStatusHooks = new List<string>() { "completed" }
                            })
                            .SetDatafolder(datafolder);

            tmp.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(tmp, "DEFAULT", new List<string>() { "completed" });

            //tmp.TestingConfiguration.WooToBiro.EndDate = new DateTime(2023, 2, 1);

            return tmp;
        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient) {
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

            var vatIdParser = new EstradaVatIdParser(client);
            var statusZavMapper = new EstradaStatusPartnerjaMapper(vatIdParser);

            var partnerMapper = new SwitchOnDavcnaPartnerInserter(client, vatIdParser,
                new PartnerWooToBiroMapper1(countryMapper, statusZavMapper, statusZavMapper),
                povoziVseAtribute: true);

            /*
            var partnerMapper = new ClassicPartnerInserter(client, 
                new PartnerWooToBiroMapper1(countryMapper, statusZavMapper, statusZavMapper),
                vatIdParser,
                povoziVsePartnerjeveAtribute: true);
            */

            var orderflow = new OrderFlow(client, partnerMapper);
            orderflow = AddRegularCase(orderflow, client, countryMapper, compositeMapper);

            return new OrderFlowProductInserterWooToBiro(orderflow, productInserter);
        }

        private OrderFlow AddRegularCase(OrderFlow orderflow, IApiClientV2 client, ICountryMapper countryMapper, CompositeWooItem_BirokratPostavkaExtractor compositeMapper) {

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
                            ).SetAdditionalParams((x) => new OrderAdditionalParams() {
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

    public class EstradaB2CIntegrationFactory
    {

        bool debug;
        string datafolder;
        public EstradaB2CIntegrationFactory(bool debug, string datafolder) {
            this.debug = debug;
            this.datafolder = datafolder;
        }

        ValidationComponents vald;

        public IIntegration BuildIntegration(IApiClientV2 client, IOutApiClient wooclient, string biroApiKey, string name = "ESTRADAOSNOVNA") {

            vald = new ValidationComponents(
                                new HardcodedCountryMapper(),
                                new NopVatParser(),
                                null,
                                null
                            );

            var tmp = new RegularIntegration(client, wooclient, biroApiKey,
                            name, new IntegrationIdentifier() { },
                            new BirokratObvezneNastavitve(new Dictionary<string, string>() {
                                { "eTrgovanjeCenaSPDFix", "true"},
                                { "eTrgovanjeDrzavaPDFix", "USA  Amerika"}
                            }),
                            includeValidator: true,
                            vald
                            )
                            .SetWooToBiro(GetWooToBiro(client, wooclient))
                            .SetBiroToWoo(null)
                            .SetPhpConfig(new PhpPluginConfig() {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = null,
                                AttachmentHook = false,
                                OrderStatusHooks = new List<string>() { "completed" }
                            })
                            .SetDatafolder(datafolder);

            tmp.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(tmp, "DEFAULT", new List<string>() { "completed" });

            return tmp;
        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient) {
            var mapper = new HardcodedCountryMapper();
            var countryMapper = mapper;


            var postavkaUtils = new BirokratPostavkaUtils(false, false);

            
            var simpleMapper = new ClassicSimpleProductMapper(postavkaUtils, client, false); 
            IWooToBiroProductMapper variableMapper = null;
            variableMapper = new ClassicVariableProductMapper(postavkaUtils, client, false, 10, new RegularSkuToSearch());


            var productInserter = new PoledancerkaWooToBiroProductInserter(wooclient, simpleMapper, variableMapper, null);
            

            List<IWooToBiroProductMapper> lst = new List<IWooToBiroProductMapper>();

            
            lst.Add(simpleMapper);
            lst.Add(variableMapper);
            var compositeMapper = new CompositeWooItem_BirokratPostavkaExtractor(client, lst, true);
            

            // vnos kupcev

            var vatIdParser = new NopVatParser();
            var statusZavMapper = new B2CStatusPartnerjaMapper();

            var partnerMapper = new SwitchOnDavcnaPartnerInserter(client, vatIdParser,
                new PartnerWooToBiroMapper1(countryMapper, statusZavMapper, statusZavMapper),
                povoziVseAtribute: true);

            var orderflow = new OrderFlow(client, partnerMapper);
            orderflow = AddFlow(orderflow, client, countryMapper, compositeMapper);

            return new OrderFlowProductInserterWooToBiro(orderflow, productInserter);
        }

        private OrderFlow AddFlow(OrderFlow orderflow, 
            IApiClientV2 client, 
            ICountryMapper countryMapper, 
            CompositeWooItem_BirokratPostavkaExtractor compositeMapper) {

            var vatAddition = new PriceMultiplierByVATRatio_PostavkaAddOp(new BirokratPostavkaUtils(false), countryMapper,
                 (order) => true);

            var addops = new List<IAdditionalOperationOnPostavke>() {
                                vatAddition,
                                new CouponPercent_PostavkeAddOp(),
                                new CouponFixedCart_PostavkeAddOp(client, new List<IAdditionalOperationOnPostavke>() { vatAddition }),
                                new Shipping_PostavkaAddOp(client, "4    22 DDV osnovna stopnja            Storitev",
                                    new List<IAdditionalOperationOnPostavke>() { })
            };

            orderflow.AddOrderFlowStage(
                        new OrderCondition() { Status = new List<string> { "completed" }, PaymentMethod = new List<string>() { "stripe" } },
                        new DocumentInsertionOrderOperationCR(
                            new DocumentInsertion(client,
                                    BirokratDocumentType.RACUN,
                                    compositeMapper,
                                    addops,
                                    countryMapper // country mapper!
                            ).SetAdditionalParams((x) => new OrderAdditionalParams() {
                                CountryMapper = countryMapper,
                                AdditionalNumber = x.Data.Id + "",
                                ExternalUniqueIdentifier = x.Data.Number,
                                SourceDocumentType = BirokratDocumentType.UNASSIGNED,
                                SourceDocumentNumberExtractor = null
                            }),
                            BirokratDocumentType.RACUN,
                            new DocumentParametersModifierOrderOperationCR(client, EstradaOrderParams.Get(), countryMapper,
                                new FiscalizationOrderOperation(client, vald, 
                            !debug ? null : new SaveDocumentOrderOperationCR(client,
                            null,
                            datafolder)))
                        )
                    );

            orderflow.AddOrderFlowStage(
                        new OrderCondition() { Status = new List<string> { "completed" }, PaymentMethod = new List<string>() { "bacs" } },
                        new DocumentInsertionOrderOperationCR(
                            new DocumentInsertion(client,
                                    BirokratDocumentType.RACUN,
                                    compositeMapper,
                                    addops,
                                    countryMapper // country mapper!
                            ).SetAdditionalParams((x) => new OrderAdditionalParams() {
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


    class EstradaOrderParams {

        public static List<DocumentParameterCommand> Get() {
            List<DocumentParameterCommand> operations = new List<DocumentParameterCommand>();
            operations.AddRange(Single());
            return operations;
        }

        public static List<DocumentParameterCommand> Single() {
            List<DocumentParameterCommand> operations = new List<DocumentParameterCommand>();



            Func<Dictionary<string, object>, bool> isSloCountry = (data) => new string[] { "SI", "SLO" }.ToList().Contains(data["wooshippingcountry"]);
            Func<Dictionary<string, object>, bool> isHrCountry = (data) => new string[] { "HR", "HRV" }.ToList().Contains(data["wooshippingcountry"]);

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("Klavzula")
                    .SetCondition(new ShippingCountry(new string[] { "SI", "SLO" }.ToList()))
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Const("Pri plačilu se sklicujte na številko '#STEVILKA#'!\r"
                                    + "Prosimo, plačajte znesek po tem računu do njegove zapadlosti. V primeru plačilne zamude zaračunavamo zakonske zamudne obresti.\r"
                                    + "\r"
                                    + "Način plačila: nakazilo na poslovni račun IBAN SI56 2900 0005 2251 041\r"))
                    .Build());

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("Klavzula")
                    .SetCondition(new ShippingCountry(new string[] { "HR", "HRV" }.ToList()))
                    .SetOperation(ParameterOperation.SET)
                    .SetValue((new Const("Pri plačilu se sklicujte na številko '#STEVILKA#'!\r"
                        + "Prosimo, plačajte znesek po tem računu do njegove zapadlosti. V primeru plačilne zamude zaračunavamo zakonske zamudne obresti.\r"
                        + "\r"
                        + "Način plačila: nakazilo na poslovni račun IBAN SI56 2900 0005 2251 041\r")))
                    .Build());

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("Klavzula")
                    .SetCondition(new And(
                                    new ShippingCountry(new string[] { "HR", "HRV" }.ToList()),
                                    new VatExempt()))
                    .SetOperation(ParameterOperation.APPEND)
                    .SetValue(new Const("\rDDV ni obračunan po 1. odst. 46. člena ZDDV-1\r"))
                    .Build());

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("DrzavaDDV")
                    .SetCondition(new Not(new ShippingCountry(new string[] { "SI", "SLO" }.ToList())))
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new BiroShippingCountry())
                    .Build());

            var cond = new And(new Not(new ShippingCountry(new string[] { "SI", "SLO" }.ToList())),
                               new And(new Not(new VatExempt()), 
                                       new ShippingCountry(Tools.EUCountryCodes().ToList()))); 

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("cmbVrstaProdaje")
                    .SetCondition(cond)
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Const("e-Trgovanje"))
                    .Build());


            var cond1 = new And(new VatExempt(),
                                new And(new ShippingCountry(Tools.EUCountryCodes().ToList()),
                                        new Not(new ShippingCountry(new string[] { "SI", "SLO" }.ToList()))));
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("cmbVrstaProdaje")
                    .SetCondition(cond1)
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Const("Oproščena dobava in dobava v članice EU (tudi izvoz)"))
                    .Build());

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("cmbVrstaProdaje")
                    .SetCondition(new Not(new ShippingCountry(Tools.EUCountryCodes().ToList())))
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Const("Oproščena dobava in dobava v članice EU (tudi izvoz)")) //"Oproščena dobava in dobava v članice EU (tudi izvoz)"
                    .Build());

            return operations;
        }

    }

    /*
     www.lycon.si

    - Vodilo je samo dase cifre med orderjem in racunom ujemajo

    - tipi izdelkov
      - enostavni - sku kode = sifra
      - variabilni - sku koda variacije = sifra izdelka

    - ko je status completed se racun kreira
    - samo ena metoda placila nakazilo na TRR



    Vse kar se lahko zgodi glede stranke:
    - ce je slovenec - dobava blaga in storitev
    - ce je hrvat - ce je zavezanec - oproscena dobava
    - ce je hrvat - ni zavezanec - eTrgovanje 25%

    - V dodatno stevilko gre ID narocila

    - Nimajo kuponov

    - Shipping postavka ista kot na narocilo


    - Hardcode mappinge iz SI in HR




    PL RACUNA:
    - cmbPredloga = Privzeto
    - klavzule ne spreminjamo
     */
}
