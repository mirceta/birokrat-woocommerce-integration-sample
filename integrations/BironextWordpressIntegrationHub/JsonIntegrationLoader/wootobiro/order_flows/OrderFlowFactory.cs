using BirokratNext;
using core.customers.zgeneric;
using JsonIntegrationLoader.order_flows.order_operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonIntegrationLoader.order_flows {
    /*
     {
        "Type": "ORDER_CHANGE_EVENT"
        "Conditions": {
        "Status": [ "processing" ],
        "PaymentType": null
        },
        "PartnerCreationCountryMapper": "COUNTRY_MAPPER",
        "FlowSequence": [
        {
            "Type": "DocumentInsertion",
            "Params": {
            "DocumentType": "racun",
            "Mapper": "COMPOSITE_MAPPER",
            "OperationsOnPostavke": [
                {
                    "Type": "CommentAddVarAttrs"
                },
                {
                    "Type": "CouponPercent"
                },
                {
                    "Type": "CouponFixedCart"
                },
                {
                    "Type": "Shipping"
                }
            ],
            "CountryMapper": "COUNTRY_MAPPER",
            "AdditionalParams": {
                "CountryMapper": "COUNTRY_MAPPER",
                "AdditionalNumber": "$$$ORDER_ID$$$",
                "ExternalUniqueIdentifier": "$$$ORDER_NUMBER$$$",
                "SourceDocumentType": "",
                "SourceDocumentNumberExtractor":  ""
            }
            }
        },
        {
            "Type": "DancerkaOrderModification",
            "Params": {}
        },
        {
            "Type": "SaveDocument",
            "Params": {}
        }
        ]
    },
    {
        "Type": "ATTACHMENT_EVENT",
        "Conditions": { same same },
        "DocumentType": "racun"
    }
     */
    class OrderFlowFactory {

        ApiClientV2 biroClient;
        DependencyStore dependencyStore;

        public OrderFlowFactory(ApiClientV2 client, DependencyStore dependencyStore) {
            biroClient = client;
            this.dependencyStore = dependencyStore;
        }

        public OrderFlow Build(OrderFlowSpecification spec) {
            
            var cond = BuildOrderCondition(spec.Conditions);
            var chain = BuildStage_Chain(spec.FlowSequence, null);

            var countryMapper = dependencyStore.countryMappers[spec.PartnerCreationCountryMapper];

            var of = new OrderFlow(biroClient, null);
            of.AddOrderFlowStage(cond, chain);
            return of;

        }

        private OrderCondition BuildOrderCondition(Dictionary<string, string[]> conditions) {

            List<string> paymentTypes = new List<string>();
            if (conditions["PaymentType"] != null) {
                paymentTypes = conditions["PaymentType"].ToList();
            }

            List<string> statuses = new List<string>();
            if (conditions["Status"] != null) {
                statuses = conditions["Status"].ToList();
            }

            return new OrderCondition() {
                Status = statuses,
                PaymentMethod = paymentTypes
            };
        }

        private IOrderOperationCR BuildStage_Chain(object[] flowElements, IOrderOperationCR next) {

            if (flowElements.Length == 0)
                return next;

            object flowElement = flowElements[flowElements.Length - 1];

            string some = JsonConvert.SerializeObject(flowElement);
            var flowEl = JsonConvert.DeserializeObject<FlowElement>(some);

            IOrderOperationCR result = BuildStage_Chain_Element(flowEl, next);
            
            if (flowElements.Length > 1) {
                flowElements = flowElements.Take(flowElements.Length - 1).ToArray();
            } else {
                flowElements = new object[] { };
            }

            return BuildStage_Chain(flowElements, result);
        }

        private IOrderOperationCR BuildStage_Chain_Element(FlowElement flowElement, IOrderOperationCR next) {
            if (flowElement.Type == "DocumentInsertion") {
                return new DocumentInsertionFactory(biroClient, dependencyStore).Build(flowElement.Params, next);
            } else if (flowElement.Type == "DocumentOrderModification") {
                return new DancerkaOrderModificationFactory(biroClient, dependencyStore).Build(flowElement.Params, next);
            } else if (flowElement.Type == "SaveDocument") {
                return new SaveDocumentOrderSpecificationFactory(biroClient, dependencyStore).Build(flowElement.Params, next);
            } else {
                throw new Exception("OrderFlow chain element type not recognized!");
            }
        }
    }

    class FlowElement { 
        public string Type { get; set; }
        public object Params { get; set; }
    }
}
