using BirokratNext;
using core.customers.poledancerka;
using core.customers.poledancerka.mappers;
using core.logic.mapping_woo_to_biro;
using System;
using System.Collections.Generic;
using tests.tools;

namespace mock_proj {
    class Program {
        static void Main(string[] args) {

            bool isb2b = true;
            ApiClientV2 client = new ApiClientV2("http://localhost:19000/api", "dJ/rjL578pbVlbdcLoudFRBDmfOe9wW+4iqwP2CuOaI=");
            var simpleMapper = new ClassicSimpleProductMapper(new BirokratPostavkaUtils(false), client, false);
            IWooToBiroProductMapper variableMapper = null;
            IWooToBiroProductMapper complexMapper = null;

            new WooOrderToBiroDocumentComparator().Compare(null, null, null);
        }
    }
}
