using BirokratNext;
using core.customers.zgeneric;
using core.logic.mapping_woo_to_biro.order_operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonIntegrationLoader.order_flows.order_operations {
    class SaveDocumentOrderSpecificationFactory {

        ApiClientV2 biroClient;
        DependencyStore dependencyStore;

        public SaveDocumentOrderSpecificationFactory(ApiClientV2 client, DependencyStore dependencyStore) {
            biroClient = client;
            this.dependencyStore = dependencyStore;
        }

        public IOrderOperationCR Build(object pars, IOrderOperationCR next) {

            string x = JsonConvert.SerializeObject(pars);
            var parse = JsonConvert.DeserializeObject<SaveDocumentOrderSpecificationPars>(x);

            return new SaveDocumentOrderOperationCR(biroClient, next, parse.Filepath);
        }

    }

    class SaveDocumentOrderSpecificationPars { 
        public string Filepath { get; set; }
    }
}

/*
 new SaveDocumentOrderOperationCR(client,
                            null, //new ObvestiloOPotekliZalogiOrderOperationCR(client, wooclient, null),
                            @"C:\Users\km\Desktop\playground\bironext-woocommerce-integration\BironextWordpressIntegrationHub\BiroWoocommerceHubTests\jsons\orders\spica\spicaproofpdfs"
                        )
 */