using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using core.logic.common_woo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{
    public class DavcnaWithVariablePrefixSearcher : IWooToBiroPartnerSearcher
    {

        /*
         Match by davcna stevilka with the following rules:
        - if davcna has no prefix, it will match it to EXACTLY this one or nothing!
        - if davcna has a prefix, it will remove all whitespace and match it EXACTLY like that.

        This means:
        - Suppose we have OIB 0100101 on db
            - OIB 0100101 will find no matches
            - OIB0100101 will find no matches
        - Suppose we have OIB0100101 on db
            - OIB 0100101 will be matched with it
            - OIB0100101 will be matched with it


        When do you use this mapper?
        - If you have a situation where you have the same partner added twice, once with whitespace
          and once without whitespace. You need to be able to return the same partner EVERY TIME
          this happens!
         */

        IApiClientV2 client;
        IVatIdParser vatIdParser;

        public DavcnaWithVariablePrefixSearcher(IApiClientV2 client,
            IVatIdParser vatIdParser) {
            this.client = client;
            this.vatIdParser = vatIdParser;
        }

        public async Task<Dictionary<string, object>> MatchWooToBiroUser(WoocommerceOrder order, Dictionary<string, string> additionalInfo) {

            string davcna = await vatIdParser.Get(order);
            if (!string.IsNullOrEmpty(davcna)) {
                return await searchByDavcna(davcna);
            }
            return null;
        }

        async Task<Dictionary<string, object>> searchByDavcna(string davcna_original) {
            List<Dictionary<string, object>> results = null;

            string davcna_no_prefix = OnlyDigits(davcna_original);
            results = await GetDavcnaMatch_DisregardWhitespace(davcna_original, results, davcna_no_prefix);

            if (results == null || results.Count == 0)
                return null;

            // try find exact match
            var match = results.Where(x => (string)x["Davčna številka"] == davcna_original).ToList();

            if (match.Count == 1) {
                return match[0];
            } else {
                // else take first one
                return null;
            }
        }

        private async Task<List<Dictionary<string, object>>> GetDavcnaMatch_DisregardWhitespace(string davcna_original, List<Dictionary<string, object>> results, string davcna_no_prefix) {

            Func<string, string> rmws = (x) => x.Replace(" ", "");
            results = await PartnerSearchHelper.findByDavcna(client, 
                $"*{davcna_no_prefix}*",
                (x) => (rmws(((string)x["Davčna številka"])) == $"{rmws(davcna_original)}"));

            return results;
        }

        private static string OnlyDigits(string davcna_original) {
            return new string(davcna_original.Where(c => char.IsDigit(c)).ToArray());
        }



    }
}
