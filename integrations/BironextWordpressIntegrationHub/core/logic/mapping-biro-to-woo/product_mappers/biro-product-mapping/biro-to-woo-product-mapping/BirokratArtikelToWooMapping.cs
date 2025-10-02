using BirokratNext;
using BiroWoocommerceHubTests;
using System;
using System.Collections.Generic;
using System.Text;

namespace core.tools
{
    public class BirokratArtikelToWooMapping
    {

        protected IOutApiClient wooclient;
        protected IApiClientV2 client;

        public BirokratArtikelToWooMapping(IApiClientV2 client, IOutApiClient wooclient) {
            this.wooclient = wooclient;
            this.client = client;
        }
    }
}
