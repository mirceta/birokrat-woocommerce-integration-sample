using BirokratNext;
using BironextWordpressIntegrationHub;
using core.customers.zgeneric;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using core.tools.wooops;
using JsonIntegrationLoader.order_flows.document_insertion;
using JsonIntegrationLoader.utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonIntegrationLoader.order_flows {

    /*
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
                "SourceDocumentType": "dobavnica",
                "SourceDocumentNumberExtractor":  "BY_ADDITIONAL_NUMBER"
            }
        }
    }

     */

    class DocumentInsertionFactory {

        ApiClientV2 biroClient;
        DependencyStore dependencyStore;

        public DocumentInsertionFactory(ApiClientV2 client, DependencyStore dependencyStore) {
            biroClient = client;
            this.dependencyStore = dependencyStore;
        }

        public IOrderOperationCR Build(object pars, IOrderOperationCR next) {

            string x = JsonConvert.SerializeObject(pars);
            var parse = JsonConvert.DeserializeObject<DocumentInsertionParameters>(x);

            var mapper = dependencyStore.postavkaExtractors[parse.Mapper];
            var countryMapper = dependencyStore.countryMappers[parse.CountryMapper];
            var additionalCountryMapper = dependencyStore.countryMappers[parse.AdditionalParams.CountryMapper];

            var additionalPostavkeOperations = new AdditionalPostavkeOperationFactory(biroClient, dependencyStore).Get(parse.OperationsOnPostavke);



            var result = new DocumentInsertionOrderOperationCR(
                new DocumentInsertion(biroClient,
                        parse.DocumentType,
                        mapper,
                        additionalPostavkeOperations,
                        countryMapper // country mapper!
                ).SetAdditionalParams((x) => 
                    {

                        string additionalNumber = OrderAttributeTemplateParser.Parse(parse.AdditionalParams.AdditionalNumber, x);
                        string externalUniqueIdentifier = OrderAttributeTemplateParser.Parse(parse.AdditionalParams.ExternalUniqueIdentifier, x);

                        string srcDocType = parse.AdditionalParams.SourceDocumentType;


                        IDocumentNumberGetter docnumextractor = null;
                        if (string.IsNullOrEmpty(parse.AdditionalParams.SourceDocumentNumberExtractor)) { }
                        else if (parse.AdditionalParams.SourceDocumentNumberExtractor == "BY_ADDITIONAL_NUMBER") {
                            // This is the only reasonable type
                            docnumextractor = new DocumentNumberGetter_ByOrderAttributeTemplate(biroClient, parse.AdditionalParams.AdditionalNumber);
                        } else {
                            throw new Exception("SourceDocumentNumberExtractor not recognized!");
                        }
                        

                        return new OrderAdditionalParams() {
                            CountryMapper = additionalCountryMapper,
                            AdditionalNumber = additionalNumber,
                            ExternalUniqueIdentifier = externalUniqueIdentifier,
                            SourceDocumentType = srcDocType,
                            SourceDocumentNumberExtractor = docnumextractor
                        };
                    }
                ),
                parse.DocumentType,
                next);

            return result;
        }

    }

    class DocumentInsertionParameters {
        public string DocumentType { get; set; }
        public string Mapper { get; set; }
        public string CountryMapper { get; set; }
        public AdditionalParams AdditionalParams { get; set; }
        public object OperationsOnPostavke { get; set; }
    }

    class AdditionalParams {
        public string CountryMapper { get; set; }
        public string AdditionalNumber { get; set; }
        public string ExternalUniqueIdentifier { get; set; }
        public string SourceDocumentType { get; set; }
        public string SourceDocumentNumberExtractor { get; set; }
    }
}