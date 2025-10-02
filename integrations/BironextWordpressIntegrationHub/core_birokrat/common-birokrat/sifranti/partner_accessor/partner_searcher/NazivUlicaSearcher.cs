using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{
    public class NazivUlicaSearcher : IWooToBiroPartnerSearcher
    {
        IPartnerWooToBiroMapper attributeMapper;
        IApiClientV2 client;
        public NazivUlicaSearcher(IPartnerWooToBiroMapper attributeMapper,
            IApiClientV2 client) {
            this.attributeMapper = attributeMapper;
            this.client = client;
        }

        public async Task<Dictionary<string, object>> MatchWooToBiroUser(WoocommerceOrder order, Dictionary<string, string> additionalInfo) {
            List<Dictionary<string, object>> results = null;
            string name = await attributeMapper.GetNaziv(order);
            results = await PartnerSearchHelper.findmatches(client, name, (x) => (string)x["Partner"] == name);

            if (results != null && results.Count > 0) {
                return TryMatchByUlica(order, results);
            } else { // no results
                return null;
            }
        }

        private Dictionary<string, object> TryMatchByUlica(WoocommerceOrder order, List<Dictionary<string, object>> results) {
            var some = results.Where(x => (string)x["Ulica"] == attributeMapper.GetUlica(order).GetAwaiter().GetResult()).ToList();
            if (some.Count > 0) {
                return some[0]; // correct match
            } else {
                return null;
            }
        }
    }
}
