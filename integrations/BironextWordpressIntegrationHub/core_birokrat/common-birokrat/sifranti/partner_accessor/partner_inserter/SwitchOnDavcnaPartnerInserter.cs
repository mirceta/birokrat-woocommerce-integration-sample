using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using core.logic.common_woo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{
    public class SwitchOnDavcnaPartnerInserter : ISifrantPartnerjevInserter
    {

        OneSearcherPartnerInserter withDavcna;
        OneSearcherPartnerInserter withoutDavcna;


        bool povoziVseAtribute = false;
        IPartnerWooToBiroMapper mapper;
        IVatIdParser vatIdParser;
        IApiClientV2 client;

        public SwitchOnDavcnaPartnerInserter(IApiClientV2 client, IVatIdParser vatIdParser, IPartnerWooToBiroMapper mapper, bool povoziVseAtribute) {
            this.client = client;
            this.vatIdParser = vatIdParser;
            this.mapper = mapper;
            this.povoziVseAtribute = povoziVseAtribute;
            withDavcna = new OneSearcherPartnerInserter(client, vatIdParser, mapper,
                    new DavcnaWithVariablePrefixSearcher(client, vatIdParser), povoziVseAtribute);
            withoutDavcna = new OneSearcherPartnerInserter(client, vatIdParser, mapper,
                    new NazivUlicaSearcher(mapper, client), povoziVseAtribute);
        }

        public async Task<string> EnforceWoocommerceBillingPartnerCreated(WoocommerceOrder order, Dictionary<string, string> additionalInfo) {

            string davcna = await vatIdParser.Get(order);
            Console.WriteLine(davcna);
            if (!string.IsNullOrEmpty(davcna)) { 
                additionalInfo = fillAdditionalInfo(additionalInfo, davcna);
                return await withDavcna.EnforceWoocommerceBillingPartnerCreated(order, additionalInfo);
            } else {
                return await withoutDavcna.EnforceWoocommerceBillingPartnerCreated(order, additionalInfo);
            }

        }

        private Dictionary<string, string> fillAdditionalInfo(Dictionary<string, string> additionalInfo, string davcna) {
            if (additionalInfo == null) {
                additionalInfo = new Dictionary<string, string>();
            }

            additionalInfo["VATID"] = davcna;
            return additionalInfo;
        }
    }
}
