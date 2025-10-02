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
using core.logic.mapping_woo_to_biro;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using core.logic.mapping_woo_to_biro.order_operations;
using core.logic.mapping_woo_to_biro.order_operations.pl;
using core.logic.mapping_woo_to_biro.product_ops;
using core.structs;
using core.tools.birokratops;
using core.tools.wooops;
using core.tools.zalogaretriever;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using tests.tools;

namespace allintegrations.customers.poledancerka
{
    public class PoledancerkaIntegrationFactory{


        bool debug;
        string datafolder;
        DancerkaOrderModificationFactory orderModificationFactory;
        public PoledancerkaIntegrationFactory(bool debug, string datafolder, DancerkaOrderModificationFactory factory) {
            this.debug = debug;
            this.datafolder = datafolder;
            this.orderModificationFactory = factory;
        }

        public async Task<IIntegration> BuildIntegration(IApiClientV2 client, IOutApiClient wooclient, string biroApiKey, bool isb2b, string name = "POLEDANCERKAB2BTEST") {

            var mapper = new WooToBiroCountryMapper(client);
            mapper.Setup();

            var integ = new RegularIntegration(client, wooclient, biroApiKey, name, null,
                            new BirokratObvezneNastavitve(new Dictionary<string, string>() {
                                            { "eTrgovanjeCenaSPDFix", "true"},
                                            { "eTrgovanjeDrzavaPDFix", "US"}
                                        }), true,
                                new ValidationComponents(
                                    mapper,
                                    new VatNumberParser(),
                                    null,
                                    new PerPartesZalogaRetriever(null, null, false)
                                )
                            )
                            .SetWooToBiro(GetWooToBiro(client, wooclient, isb2b, mapper))
                            .SetBiroToWoo(null)
                            .SetPhpConfig(new PhpPluginConfig() {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = new List<string>() { "completed" },
                                AttachmentHook = true,
                                OrderStatusHooks = new List<string>() { "processing" }
                            })
                            .SetDatafolder(datafolder);


            string companyName = name;
            integ.TestingConfiguration = new TestingConfiguration(
                new TestingConfigurationBiroToWoo("SPICA"),
                new TestingConfigurationWooToBiro(new List<string>() { "processing" }, companyName, $"{companyName}/narocila"));

            return integ;
        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient, bool isb2b, ICountryMapper mapper) {
            
            var countryMapper = mapper;


            var simpleMapper = new ClassicSimpleProductMapper(new BirokratPostavkaUtils(false), client, true);
            IWooToBiroProductMapper variableMapper = null;
            IWooToBiroProductMapper complexMapper = null;
            
            if (isb2b) {
                variableMapper = new ClassicVariableProductMapper(new BirokratPostavkaUtils(false), client, true, 3, new PoledancerkaSkuToSearch());
                complexMapper = new DancerkaComplexProductMapper(new BirokratPostavkaUtils(false), client, true, 3);
            } else {
                variableMapper = new ClassicVariableProductMapper(new BirokratPostavkaUtils(false), client, true, 2, new PoledancerkaSkuToSearch());
                complexMapper = new DancerkaComplexProductMapper(new BirokratPostavkaUtils(false), client, true, 2);
            }

            var productInserter = new PoledancerkaWooToBiroProductInserter(wooclient, simpleMapper, variableMapper, complexMapper);

            List<IWooToBiroProductMapper> lst = new List<IWooToBiroProductMapper>();
            lst.Add(simpleMapper);
            lst.Add(variableMapper);
            lst.Add(complexMapper);
            var compositeMapper = new CompositeWooItem_BirokratPostavkaExtractor(client, lst, !isb2b);


            var vatIdParser = new VatNumberParser();
            var statusZavMapper = new DancerkaB2BStatusPartnerjaMapper(vatIdParser);

            var partnerInserter = new SwitchOnDavcnaPartnerInserter(client, vatIdParser,
                new PartnerWooToBiroMapper1(countryMapper, statusZavMapper, statusZavMapper),
                povoziVseAtribute: true);



            var orderflow = new OrderFlow(client, partnerInserter);
            if (!isb2b) {
                orderflow = AddRegularCase(orderflow, client, countryMapper, compositeMapper);
            } else {
                orderflow = AddB2BCase(orderflow, true, client, countryMapper, compositeMapper);
                orderflow = AddB2BCase(orderflow, false, client, countryMapper, compositeMapper);
            }
            return new OrderFlowProductInserterWooToBiro(orderflow, productInserter);
        }

        private OrderFlow AddB2BCase(OrderFlow orderflow, bool davcniZavezanec, IApiClientV2 client, ICountryMapper countryMapper, CompositeWooItem_BirokratPostavkaExtractor compositeMapper) {

            var multiplyIfOproscenaDobava = new PriceMultiplierByVAT_ForOproscenaDobava_PostavkaAddOp(new BirokratPostavkaUtils(false), countryMapper,
                 new OrX(new VatExemptX(), new NotX(new ShippingCountryIsEuX())));

            var postavkeAdditionalOps = new List<IAdditionalOperationOnPostavke>();
            postavkeAdditionalOps.Add(new CommentAddVarAttrs_PostavkaAddOp(true));
            postavkeAdditionalOps.Add(new DancerkaFixComplexComment());
            
            postavkeAdditionalOps.Add(multiplyIfOproscenaDobava);
            postavkeAdditionalOps.Add(new CouponPercent_PostavkeAddOp());

            postavkeAdditionalOps.Add(new CouponFixedCart_PostavkeAddOp(client, 
                new List<IAdditionalOperationOnPostavke>() { multiplyIfOproscenaDobava }));
            postavkeAdditionalOps.Add(new Shipping_PostavkaAddOp(client, "4    22 DDV osnovna stopnja            Storitev",
                new List<IAdditionalOperationOnPostavke>() { multiplyIfOproscenaDobava }));
            


            string isVatExempt = davcniZavezanec ? "yes" : "no";

            orderflow.AddOrderFlowStage(
                        new OrderCondition() { Status = new List<string> { "processing" }, PaymentMethod = null, IsVatExempt = isVatExempt },
                        new DocumentInsertionOrderOperationCR(
                            new DocumentInsertion(client,
                                    BirokratDocumentType.RACUN,
                                    compositeMapper,
                                    postavkeAdditionalOps,
                                    countryMapper // country mapper!
                            ).SetAdditionalParams((x) => new OrderAdditionalParams() {
                                CountryMapper = countryMapper,
                                AdditionalNumber = x.Data.Id + "",
                                ExternalUniqueIdentifier = x.Data.Number,
                                SourceDocumentType = BirokratDocumentType.UNASSIGNED,
                                SourceDocumentNumberExtractor = null
                            }),
                            BirokratDocumentType.RACUN,
                            orderModificationFactory.Get(true, davcniZavezanec, client, countryMapper,
                            !debug ? null : new SaveDocumentOrderOperationCR(client,
                            null,
                            datafolder))
                        )
                    );

            orderflow.AddAttachmentFlowStage(
                new OrderCondition() { Status = new List<string> { "completed" }, PaymentMethod = null },
                new DocumentNumberGetter_ByOrderAttributeTemplate(client, "$$$ORDER_ID$$$", BirokratDocumentType.RACUN,
                     new BiroDocumentPdfGetter(client, null)));

            return orderflow;
        }

        private OrderFlow AddRegularCase(OrderFlow orderflow, IApiClientV2 client, ICountryMapper countryMapper, CompositeWooItem_BirokratPostavkaExtractor compositeMapper) {

            var multiplyIfOproscenaDobava = new PriceMultiplierByVAT_ForOproscenaDobava_PostavkaAddOp(new BirokratPostavkaUtils(false), countryMapper,
                 new NotX(new ShippingCountryIsEuX()));

            var addops = new List<IAdditionalOperationOnPostavke>() {
                                new CommentAddVarAttrs_PostavkaAddOp(true),
                                new DancerkaFixComplexComment(),
                                multiplyIfOproscenaDobava,
                                new CouponPercent_PostavkeAddOp(),
                                new CouponFixedCart_PostavkeAddOp(client, new List<IAdditionalOperationOnPostavke>() { multiplyIfOproscenaDobava }),
                                new Shipping_PostavkaAddOp(client, "4    22 DDV osnovna stopnja            Storitev",
                                    new List<IAdditionalOperationOnPostavke>() { multiplyIfOproscenaDobava }) };
                                    

            orderflow.AddOrderFlowStage(
                        new OrderCondition() { Status = new List<string> { "processing" }, PaymentMethod = null },
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
                            orderModificationFactory.Get(false, true, client, countryMapper,
                            !debug ? null : new SaveDocumentOrderOperationCR(client,
                            null,
                            datafolder))
                        )
                    ); ;

            orderflow.AddAttachmentFlowStage(
                new OrderCondition() { Status = new List<string> { "completed" }, PaymentMethod = null },
                new DocumentNumberGetter_ByOrderAttributeTemplate(client, "$$$ORDER_ID$$$", BirokratDocumentType.RACUN,
                     new BiroDocumentPdfGetter(client, null)));

            if (debug) {
                orderflow.AddAttachmentFlowStage(
                    new OrderCondition() { Status = new List<string> { "processing" }, PaymentMethod = null },
                    new DocumentNumberGetter_ByOrderAttributeTemplate(client, "$$$ORDER_ID$$$", BirokratDocumentType.RACUN,
                         new BiroDocumentPdfGetter(client, null)));
            }

            return orderflow;
        }
    }

    public class DancerkaOrderModificationFactory {

        bool isnew;
        
        public DancerkaOrderModificationFactory(bool isnew) {
            this.isnew = isnew;
        }

        public IOrderOperationCR Get(bool b2b, bool davcniZavezanec, IApiClientV2 client, ICountryMapper mapper, IOrderOperationCR next) {
            if (isnew) {
                return new DocumentParametersModifierOrderOperationCR(client, PoledancerkaOrderParams.Get(b2b), mapper, next);
            } 
            else if (!b2b) {
                return new DancerkaOrderModificationOrderOperationCR(client, mapper, next);
            } 
            else {
                return new DancerkaB2BOrderModificationOrderOperationCR(client, davcniZavezanec, mapper, next);
            }
        }
    }

    class PoledancerkaOrderParams {

        public static List<DocumentParameterCommand> Get(bool b2b) {
            List<DocumentParameterCommand> operations = new List<DocumentParameterCommand>();
            operations.AddRange(Both());

            if (b2b) {
                operations.AddRange(B2B());
            } else {
                operations.AddRange(NotB2B());
            }

            return operations;
        }

        public static List<DocumentParameterCommand> Both() {
            List<DocumentParameterCommand> operations = new List<DocumentParameterCommand>();

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("cmbPredloga")
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Const("ARRacun1"))
                    .Build());


            //(order, data) => new Template("SI00 {{{stevilkaDokumenta}}}-1-{{{orderNumber}}}")
            //.SetValue((order, data) )

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("txtUvodniText")
                    .SetCondition(new ShippingCountry(new string[] { "SI", "SLO" }.ToList()))
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Template("Na osnovi naročila: #$$$ORDER_NUMBER$$$"))
                    .Build());
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("txtUvodniText")
                    .SetCondition(new Not(new ShippingCountry(new string[] { "SI", "SLO" }.ToList())))
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Template("Based on order: #$$$ORDER_NUMBER$$$"))
                    .Build());
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("cmbJezik")
                    .SetCondition(new ShippingCountry(new string[] { "SI", "SLO" }.ToList()))
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Const("002 Slovenščina"))
                    .Build());
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("cmbJezik")
                    .SetCondition(new Not(new ShippingCountry(new string[] { "SI", "SLO" }.ToList())))
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Const("003 Angleščina"))
                    .Build());

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("DrzavaDDV")
                    .SetCondition(new Not(new ShippingCountry(new string[] { "SI", "SLO" }.ToList())))
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new BiroShippingCountry())
                    .Build());
            return operations;
        }

        public static List<DocumentParameterCommand> B2B() {
            List<DocumentParameterCommand> operations = new List<DocumentParameterCommand>();

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("Klavzula")
                    .SetCondition(new ShippingCountry(new string[] { "SI", "SLO" }.ToList()))
                    .SetOperation(ParameterOperation.REPLACE)
                    .SetValue(new Const("Thank you for your custom!"))
                    .SetReplaceWith("Hvala za nakup!")
                    .Build());
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("Klavzula")
                    .SetCondition(new Tautology())
                    .SetOperation(ParameterOperation.REPLACE)
                    .SetValue(new Const("\r\rThank you for your custom!\r\r"))
                    .SetReplaceWith("")
                    .Build());



            var cond = new And(new Not(new ShippingCountry(new string[] { "SI", "SLO" }.ToList())),
                               new And(new Not(new VatExempt()),
                                       new ShippingCountry(Tools.EUCountryCodes().ToList())));

            

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("Klavzula")
                    .SetCondition(new And(new VatExempt(), new ShippingCountry(Tools.EUCountryCodes().ToList())))
                    .SetOperation(ParameterOperation.APPEND)
                    .SetValue(new Const("Oproščeno DDV po 1. točki 46. člena ZDDV-1\r"
                        + "VAT exempt under Article 138(1) of Directive 2006/112/ES\r"
                        + "\r"
                        + "Thank you for your custom!"))
                    .Build());

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("Klavzula")
                    .SetCondition(new Not(new ShippingCountry(Tools.EUCountryCodes().ToList())))
                    .SetOperation(ParameterOperation.APPEND)
                    .SetValue(new Const("Oproščeno DDV po točki a) prvega odstavka 52. člena ZDDV-1\r"
                        + "VAT exempt under Article 146(1)(a) of Directive\r"
                        + "\r"
                        + "Thank you for your custom!"))
                    .Build());


            var cnd = new And3(new Not(new VatExempt()),
                               new ShippingCountry(Tools.EUCountryCodes().ToList()),
                               new Not(new ShippingCountry(new string[] { "SI", "SLO" }.ToList())));
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("cmbVrstaProdaje")
                    .SetCondition(cnd)
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Const("e-Trgovanje"))
                    .Build());

            var cnd1 = new And3(new VatExempt(),
                               new ShippingCountry(Tools.EUCountryCodes().ToList()),
                               new Not(new ShippingCountry(new string[] { "SI", "SLO" }.ToList())));
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("cmbVrstaProdaje")
                    .SetCondition(cnd1)
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

        public static List<DocumentParameterCommand> NotB2B() {

            List<DocumentParameterCommand> operations = new List<DocumentParameterCommand>();


            operations.Add(new DocumentParameterCommand.Builder()
                    .SetCondition(new ShippingCountry(new string[] { "SI", "SLO" }.ToList()))
                    .SetFieldName("Klavzula")
                    .SetOperation(ParameterOperation.REPLACE)
                    .SetValue(new Const("Thank you for your custom!"))
                    .SetReplaceWith("Hvala za nakup!")
                    .Build());
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetCondition(new ShippingCountry(new string[] { "SI", "SLO" }.ToList()))
                    .SetFieldName("Klavzula")
                    .SetOperation(ParameterOperation.REPLACE)
                    .SetValue(new Const("Thank you for your custom!"))
                    .SetReplaceWith("Hvala za nakup!")
                    .Build());
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetCondition(new Not(new ShippingCountry(Tools.EUCountryCodes().ToList())))
                    .SetFieldName("Klavzula")
                    .SetOperation(ParameterOperation.REPLACE)
                    .SetValue(new Const("\r\rThank you for your custom!\r\r"))
                    .SetReplaceWith("")
                    .Build());
            operations.Add(new DocumentParameterCommand.Builder()
                    .SetCondition(new Not(new ShippingCountry(Tools.EUCountryCodes().ToList())))
                    .SetFieldName("Klavzula")
                    .SetOperation(ParameterOperation.APPEND)
                    .SetValue(new Const("Oproščeno DDV po točki a) prvega odstavka 52. člena ZDDV-1\r"
                        + "VAT exempt under Article 146(1)(a) of Directive\r"
                        + "\r"
                        + "Thank you for your custom!"))
                    .Build());
            operations.Add(new DocumentParameterCommand.Builder()
                   .SetFieldName("cmbVrstaProdaje")
                   .SetCondition(new Not(new ShippingCountry(Tools.EUCountryCodes().ToList())))
                   .SetOperation(ParameterOperation.SET)
                   .SetValue(new Const("Oproščena dobava in dobava v članice EU (tudi izvoz)")) //"Oproščena dobava in dobava v članice EU (tudi izvoz)"
                   .Build());

            

            operations.Add(new DocumentParameterCommand.Builder()
                    .SetFieldName("cmbVrstaProdaje")
                    .SetCondition(new And(
                                        new ShippingCountry(Tools.EUCountryCodes().ToList()),
                                new Not(new ShippingCountry(new string[] { "SI", "SLO" }.ToList()))))
                    .SetOperation(ParameterOperation.SET)
                    .SetValue(new Const("e-Trgovanje"))
                    .Build());

            return operations;
        }        
    }
}
