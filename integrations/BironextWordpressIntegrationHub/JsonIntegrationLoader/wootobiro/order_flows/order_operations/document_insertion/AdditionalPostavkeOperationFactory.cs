using BirokratNext;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonIntegrationLoader.order_flows.document_insertion {
    class AdditionalPostavkeOperationFactory {

        ApiClientV2 biroClient;
        DependencyStore dependencyStore;

        public AdditionalPostavkeOperationFactory(ApiClientV2 client, DependencyStore dependencyStore) {
            biroClient = client;
            this.dependencyStore = dependencyStore;
        }

        public List<IAdditionalOperationOnPostavke> Get(object pars) {

            string x = JsonConvert.SerializeObject(pars);
            var parse = JsonConvert.DeserializeObject<List<AdditionalOperationSpec>>(x);


            List<IAdditionalOperationOnPostavke> addops = new List<IAdditionalOperationOnPostavke>();
            foreach (var par in parse) {

                IAdditionalOperationOnPostavke some = null;
                if (par.Type == "CommentAddVarAttrs") {
                    some = new CommentAddVarAttrs_PostavkaAddOp(true);
                } else if (par.Type == "CouponPercent") {
                    some = new CouponPercent_PostavkeAddOp();
                } else if (par.Type == "CouponFixedCart") {
                    some = new CouponFixedCart_PostavkeAddOp(biroClient);
                } else if (par.Type == "Shipping") {
                    some = new Shipping_PostavkaAddOp(biroClient);
                } else {
                    throw new Exception("Additional operation type not recognized!");
                }
                addops.Add(some);

            }
            return addops;
        }
    }

    class AdditionalOperationSpec { 
        public string Type { get; set; }
        // if needed also add Params
    }
}
