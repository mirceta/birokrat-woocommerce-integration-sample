using BirokratNext;
using core.customers.zgeneric;
using core.logic.mapping_woo_to_biro.order_operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonIntegrationLoader.order_flows.order_operations {
    class DancerkaOrderModificationFactory {

        ApiClientV2 biroClient;
        DependencyStore dependencyStore;

        public DancerkaOrderModificationFactory(ApiClientV2 client, DependencyStore dependencyStore) {
            biroClient = client;
            this.dependencyStore = dependencyStore;
        }

        public IOrderOperationCR Build(object pars, IOrderOperationCR next) {

            string x = JsonConvert.SerializeObject(pars);
            var parse = JsonConvert.DeserializeObject<DancerkaOrderModificationParams>(x);

            var countryMapper = dependencyStore.countryMappers[parse.CountryMapper];

            return new DancerkaOrderModificationOrderOperationCR(biroClient, countryMapper, next);

        }
    }

    class DancerkaOrderModificationParams { 
        public string CountryMapper { get; set; }
    }
}
