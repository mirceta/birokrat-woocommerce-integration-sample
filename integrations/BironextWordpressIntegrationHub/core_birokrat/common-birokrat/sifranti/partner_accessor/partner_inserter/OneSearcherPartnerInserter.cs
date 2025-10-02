using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using core.logic.common_woo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{
    public class OneSearcherPartnerInserter : ISifrantPartnerjevInserter
    {
        IWooToBiroPartnerSearcher searcher;
        IVatIdParser vatIdParser;
        IPartnerWooToBiroMapper attributeMapper;
        IApiClientV2 client;
        bool povoziVsePartnerjeveAtribute;

        public OneSearcherPartnerInserter(IApiClientV2 client, 
            IVatIdParser parser,
            IPartnerWooToBiroMapper attributeMapper,
            IWooToBiroPartnerSearcher searcher,
            bool povoziVsePartnerjeveAtribute = false) {
            if (client == null)
                throw new ArgumentNullException("client");
            if (attributeMapper == null)
                throw new ArgumentNullException("attributeMapper");
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (searcher == null)
                throw new ArgumentNullException("searcher");
            this.searcher = searcher;
            this.attributeMapper = attributeMapper;
            this.vatIdParser = parser;
            this.client = client;
            this.povoziVsePartnerjeveAtribute = povoziVsePartnerjeveAtribute;
        }

        public async Task<string> EnforceWoocommerceBillingPartnerCreated(WoocommerceOrder order, Dictionary<string, string> additionalInfo) {

            string woodavcna = "";
            if (additionalInfo != null && additionalInfo.ContainsKey("VATID")) {
                woodavcna = additionalInfo["VATID"];
            } else {
                woodavcna = await vatIdParser.Get(order);
            }

            var match = await searcher.MatchWooToBiroUser(order, additionalInfo);

            if (match == null) {
                return await new PartnerInserterHelper(client, attributeMapper).UstvariNovegaPartnerja(order, woodavcna);
            } else if (povoziVsePartnerjeveAtribute) {
                await new PartnerInserterHelper(client, attributeMapper).PovoziObstojecegaPartnerja(order, (string)match["Oznaka"]);
            }

            return (string)match["Oznaka"];
        }
    }
}
