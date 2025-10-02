using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub;
using BironextWordpressIntegrationHub.structs;
using birowoo_exceptions;
using BiroWoocommerceHub;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.logic;
using core.customers.zgeneric;
using core.customers.zgeneric.order_operations;
using core.logic.common_woo;
using core.logic.mapping_woo_to_biro;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_extractors;
using core.logic.mapping_woo_to_biro.order_operations;
using core.logic.mapping_woo_to_biro.order_operations.pl;
using core.logic.mapping_woo_to_biro.orderflow.order_operations;
using JsonIntegrationLoader.utils;
using tests.tools;

namespace allintegrations_factories.wrappers
{

    public class OrderFlowStageBuilder : IOrderFlowStageBuilder
    {

        ISifrantPartnerjevInserter insert;
        ICountryMapper countryMapper;
        List<IAdditionalOperationOnPostavke> additionalOperationOnPostavkes;
        IApiClientV2 client;
        string datafolder;
        bool debug;
        public OrderFlowStageBuilder(bool debug, IApiClientV2 client, ICountryMapper countryMapper, string datafolder)
        {
            this.debug = debug;
            var statusZavMapper = new B2CStatusPartnerjaMapper();
            insert = new ClassicPartnerInserter(client,
                new PartnerWooToBiroMapper1(new HardcodedCountryMapper(), statusZavMapper, statusZavMapper),
                new VatNumberParser());
            this.countryMapper = countryMapper;
            this.client = client;
            this.datafolder = datafolder;
        }

        // mandatory
        public OrderFlowStageBuilder SetPostavkeOperations(bool includePostavkeComments,
                                                           bool percentCoupons,
                                                           bool fixedCartCoupons,
                                                           bool includeShipping,
                                                           bool includeHandlingOproscenaDobava)
        {
            var addops = new List<IAdditionalOperationOnPostavke>();
            if (includePostavkeComments)
                addops.Add(new CommentAddVarAttrs_PostavkaAddOp(true));
            if (percentCoupons)
                addops.Add(new CouponPercent_PostavkeAddOp());


            var futheraddops = new List<IAdditionalOperationOnPostavke>() { };
            var multiplyIfOproscenaDobava = new PriceMultiplierByVAT_ForOproscenaDobava_PostavkaAddOp(new BirokratPostavkaUtils(false), countryMapper,
                        new NotX(new ShippingCountryIsEuX()));

            if (includeHandlingOproscenaDobava)
                futheraddops.Add(multiplyIfOproscenaDobava);

            if (fixedCartCoupons)
                addops.Add(new CouponFixedCart_PostavkeAddOp(client, futheraddops));

            if (includeShipping)
                addops.Add(new Shipping_PostavkaAddOp(client,
                    "6     0 DDV oproščen promet            Storitev",
                    futheraddops,
                    sifraAddition: "11"));

            additionalOperationOnPostavkes = addops;

            return this;
        }

        // optional
        public OrderFlowStageBuilder SetPartnerInserter(ISifrantPartnerjevInserter inserter)
        {
            insert = inserter;
            return this;
        }

        // optional
        ValidationComponents validationComponents;
        public OrderFlowStageBuilder SetValidationComponents(ValidationComponents validationComponents)
        {
            this.validationComponents = validationComponents;
            return this;
        }


        OrderFlow orderFlow;
        public OrderFlowStageBuilder BuildOrderFlow()
        {
            orderFlow = new OrderFlow(client, insert);
            return this;
        }


        public OrderFlowStageBuilder AddStage(bool overrideSklicWithAdditionalNumber,
            OrderCondition orderCondition,
            BirokratDocumentType doctype, BirokratDocumentType sourceDocType, OrderAttributeTemplateParser2 parser, bool fiscallize = false)
        {
            if (orderFlow == null)
                throw new Exception("Cannot add stage before calling BuildOrderFlow");

            IOrderOperationCR orderOp = null;

            if (debug)
            {
                orderOp = new SaveDocumentOrderOperationCR(client,
                    null,
                    datafolder
                );
            }

            if (!debug && fiscallize)
            {
                if (validationComponents == null)
                    throw new IntegrationProcessingException("To fiscallize automatically, validation components must be set!");

                orderOp = new FiscalizationOrderOperation(client, validationComponents, orderOp);
            }

            if (overrideSklicWithAdditionalNumber)
            {
                var operations = new List<DocumentParameterCommand>();
                operations.Add(new DocumentParameterCommand.Builder()
                        .SetFieldName("Sklic")
                        .SetOperation(ParameterOperation.SET)
                        .SetValue(new Template2(parser, "SI00 {{{stevilkaDokumenta}}}-[[[parser]]]"))
                        .Build());
                orderOp = new DocumentParametersModifierOrderOperationCR(client, operations,
                    countryMapper, orderOp);
            }

            if (parser != null)
            {
                orderOp = new ChangeDocNumOrderOperationCR2(client, parser, orderOp);
            }

            orderOp = insertDocument(doctype, sourceDocType, parser, orderOp);

            if (parser != null)
            {
                orderOp = verifyDocDoesntExist(doctype, parser, orderOp);
            }

            orderFlow.AddOrderFlowStage(orderCondition, orderOp);

            return this;
        }

        private IOrderOperationCR insertDocument(BirokratDocumentType doctype,
            BirokratDocumentType sourceDocType,
            OrderAttributeTemplateParser2 template,
            IOrderOperationCR orderOp)
        {
            var documentInsertion = new DocumentInsertion(client,
                                        doctype,
                                        new BirokratAttributeIsOriginalOrVariationSku_BirokratPostavkaExtractor(
                                            new BirokratPostavkaUtils(false)),
                                        additionalOperationOnPostavkes,
                                        countryMapper
                                );
            if (sourceDocType != BirokratDocumentType.UNASSIGNED)
                connectWithSourceDocument(sourceDocType, template, documentInsertion);
            orderOp = new DocumentInsertionOrderOperationCR(
                    documentInsertion,
                    doctype,
                    orderOp
                );
            if (sourceDocType != BirokratDocumentType.UNASSIGNED)
            {

                orderOp = verifySourceDocExists(sourceDocType, template, orderOp);
            }

            return orderOp;
        }

        public OrderFlow GetOrderFlow()
        {
            return orderFlow;
        }

        private IOrderOperationCR verifyDocDoesntExist(BirokratDocumentType doctype, OrderAttributeTemplateParser2 template, IOrderOperationCR orderOp)
        {
            orderOp = new DocumentAlreadyExistsGuard_OrderOperationCR(
                                    new DocumentNumberGetter_ByTemplate(client, template),
                                    doctype,
                                    orderOp);
            return orderOp;
        }

        private IOrderOperationCR verifySourceDocExists(BirokratDocumentType sourceDocType, OrderAttributeTemplateParser2 template, IOrderOperationCR orderOp)
        {
            orderOp = new EnsureDocumentExists_OrderOperationCR(
                new DocumentNumberGetter_ByTemplate(client, template),
                sourceDocType,
                orderOp);
            return orderOp;
        }

        private void connectWithSourceDocument(BirokratDocumentType sourceDocType, OrderAttributeTemplateParser2 template, DocumentInsertion documentInsertion)
        {
            documentInsertion.SetAdditionalParams((x) => new OrderAdditionalParams()
            {
                CountryMapper = countryMapper,
                AdditionalNumber = x.Data.Number,
                ExternalUniqueIdentifier = x.Data.Number,
                SourceDocumentType = sourceDocType,
                SourceDocumentNumberExtractor = new DocumentNumberGetter_ByTemplate
                                        (client, template)
            });
        }
    }
}
