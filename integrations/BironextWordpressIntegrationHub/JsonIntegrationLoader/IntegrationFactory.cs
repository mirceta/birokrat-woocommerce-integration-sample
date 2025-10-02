using BirokratNext;
using BiroWoocommerceHub;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.customers.poledancerka;
using core.customers.poledancerka.mappers;
using core.customers.zgeneric;
using core.logic.mapping_woo_to_biro.document_insertion;
using JsonIntegrationLoader.order_flows;
using Newtonsoft.Json;
using si.birokrat.next.common.build;
using System;
using System.Collections.Generic;
using System.IO;


namespace JsonIntegrationLoader {

    /*
     {
    "BiroCredentials": {
      "ApiKey": "vs/rjL57PpbVlbHcLoud/RBDmfO19wW+IiqwP2CuOaI="
    },
    "WooCredentials": {
      "Address": "https://poledancerka.belvgdev.com/",
      "Ck": "ck_f5e89f3fb3a23dadb2e84c559eb94847b0078b4a",
      "Cs": "cs_23f79b73ff593ede6bd37db33d332e9172caff9f",
      "Version": "wc/v3"
    },
    "WooToBiro": {
      
    },
    "BiroToWoo": {
      
    },
    "Dependencies": {
      "OrderFlows": {
        "NAME1": {}
      },
      "PostavkaExtractors": {
        "NAME1": {
          "Type": "CompositeMapper"
        }
      },
      "CountryMappers": {
        "MAIN_MAPPER": {
          "Type": "WooToBiroCountryMapper"
        }
      }
    }
  }
     */

    class IntegrationFactory {

        ApiClientV2 biroClient;
        IOutApiClient wooClient;
        DependencyStore dependencyStore;

        public IntegrationFactory(ApiClientV2 biroClient, IOutApiClient wooClient) {
            this.biroClient = biroClient;
            this.wooClient = wooClient;
            dependencyStore = new DependencyStore();
        }

        public IIntegration ParseIntegration(IntegrationJsonSpecification spec) {

            ParseCountryMappers(spec);
            ParsePostavkaExtractors(spec);
            ParseOrderFlows(spec);
            return null;
        }

        void ParseCountryMappers(IntegrationJsonSpecification spec) {

            var main = spec.Dependencies.CountryMappers["MAIN_MAPPER"];

            if (main.Type == "WooToBiroCountryMapper") {
                var some = new WooToBiroCountryMapper(biroClient);
                dependencyStore.countryMappers["MAIN_MAPPER"] = some;
            }

        }

        void ParsePostavkaExtractors(IntegrationJsonSpecification spec) {
            
            var dic = spec.Dependencies.PostavkaExtractors;
            foreach (string key in dic.Keys) {
                var val = dic[key];
                IBirokratPostavkaExtractor extractor = null;
                if (val.Type == "Poledancerka") {
                    extractor = new PostavkaParserFactory(biroClient).Poledancerka(false);
                } else if (val.Type == "PoledancerkaB2B") {
                    extractor = new PostavkaParserFactory(biroClient).Poledancerka(true);
                }
                dependencyStore.postavkaExtractors[key] = extractor;
            }
        }

        void ParseOrderFlows(IntegrationJsonSpecification spec) {
            
            var dic = spec.Dependencies.OrderFlows;
            foreach (string key in dic.Keys) {
                var val = dic[key];
                var of = new OrderFlowFactory(biroClient, dependencyStore).Build(val);
                dependencyStore.orderFlows[key] = of;
            }

        }


    }

    public class DependencyStore {
        public Dictionary<string, ICountryMapper> countryMappers;
        public Dictionary<string, IBirokratPostavkaExtractor> postavkaExtractors;
        public Dictionary<string, OrderFlow> orderFlows;

        public DependencyStore() {
            postavkaExtractors = new Dictionary<string, IBirokratPostavkaExtractor>();
            orderFlows = new Dictionary<string, OrderFlow>();
            countryMappers = new Dictionary<string, ICountryMapper>();
        }
    }

    class IntegrationJsonSpecification {
        public Dictionary<string, object> BiroCredentials { get; set; }
        public Dictionary<string, object> WooCredentials { get; set; }
        public Dictionary<string, object> WooToBiro { get; set; }
        public Dictionary<string, object> BiroToWoo { get; set; }
        public Dependencies Dependencies { get; set; }
    }

    class Dependencies {
        public Dictionary<string, OrderFlowSpecification> OrderFlows { get; set; }
        public Dictionary<string, CountryMapperSpecification> CountryMappers { get; set; }
        public Dictionary<string, PostavkaExtractorSpecification> PostavkaExtractors { get; set; }
    }

    class CountryMapperSpecification {
        public string Type { get; set; }
    }

    class PostavkaExtractorSpecification {
        public string Type { get; set; }
    }

    class OrderFlowSpecification {
        public Dictionary<string, string[]> Conditions { get; set; }
        public object[] FlowSequence { get; set; }
        public string PartnerCreationCountryMapper { get; set; }
    }
}