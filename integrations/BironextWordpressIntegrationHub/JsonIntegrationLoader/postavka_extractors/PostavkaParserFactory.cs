using BirokratNext;
using core.customers.poledancerka;
using core.customers.poledancerka.mappers;
using core.logic.mapping_woo_to_biro;
using core.logic.mapping_woo_to_biro.document_insertion;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonIntegrationLoader {
    class PostavkaParserFactory {

        ApiClientV2 biroClient;
        public PostavkaParserFactory(ApiClientV2 client) {
            biroClient = client;
        }

        public IBirokratPostavkaExtractor Poledancerka(bool isb2b) {
            var simpleMapper = new ClassicSimpleProductMapper(new BirokratPostavkaUtils(false), biroClient, true);
            ClassicVariableProductMapper variableMapper;
            DancerkaComplexProductMapper complexMapper;
            if (isb2b) {
                variableMapper = new ClassicVariableProductMapper(new BirokratPostavkaUtils(false), biroClient, true, 3, new PoledancerkaSkuToSearch());
            } else {
                variableMapper = new ClassicVariableProductMapper(new BirokratPostavkaUtils(false), biroClient, true, 2, new PoledancerkaSkuToSearch());
            }
            if (isb2b) {
                complexMapper = new DancerkaComplexProductMapper(new BirokratPostavkaUtils(false), biroClient, true, 3);
            } else {
                complexMapper = new DancerkaComplexProductMapper(new BirokratPostavkaUtils(false), biroClient, true, 2);
            }

            List<IWooToBiroProductMapper> lst = new List<IWooToBiroProductMapper>();
            lst.Add(simpleMapper);
            lst.Add(variableMapper);
            lst.Add(complexMapper);
            var compositeMapper = new CompositeWooItem_BirokratPostavkaExtractor(biroClient, lst, !isb2b);
            return compositeMapper;
        }

    }
}
