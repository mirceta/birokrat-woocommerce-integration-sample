using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.customers.zgeneric;
using core.customers.zgeneric.order_operations;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_extractors;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.customers {
    class WooToBiro : IWooToBiro {


        IApiClientV2 client;
        IOutApiClient wooclient;
        OrderFlow flow;
        public WooToBiro(IApiClientV2 client, 
            IOutApiClient wooclient,
            OrderFlow flow) {
            this.client = client;
            this.flow = flow;
        }

        public Task OnArticleAdded(WoocommerceProduct product) {
            throw new NotImplementedException();
        }

        public Task OnArticleAddedRaw(string product_id, string variation_id) {
            throw new NotImplementedException();
        }

        public Task OnArticleChanged(Dictionary<string, WoocommerceProduct> product) {
            throw new NotImplementedException();
        }

        public Task OnArticleChangedRaw(string product_id, string variation_id) {
            throw new NotImplementedException();
        }

        public Task<object> OnAttachmentRequest(string order) {
            throw new NotImplementedException();
        }

        public async Task<object> OnOrderStatusChanged(string body) {
            throw new NotImplementedException();
        }

        Task<Dictionary<string, object>> IWooToBiro.OnOrderStatusChanged(string order)
        {
            throw new NotImplementedException();
        }
    }
}
