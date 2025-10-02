using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiroWoocommerceHubTests.tools
{
    class WooAttributeTermCache {

        IOutApiClient wooclient;
        public WooAttributeTermCache(IOutApiClient wooclient) {
            this.wooclient = wooclient;
        }

        Dictionary<string, string[]> terms = new Dictionary<string, string[]>();
        public string GetOrAddTerm(string attr_id, string term) {
            // WARNING: THIS WILL FAIL IN THE CASE:
            // 1. YOU LOAD UP THE TERMS
            // 2. CUSTOMER ADDS THE TERM YOU WANTED TO ADD TO WOOCOMMERCE
            // 3. YOU TRY TO ADD THE TERM BUT IT'S ALREADY ADDED

            string some = "";
            if (!terms.ContainsKey(attr_id)) {


                /*
                some = wooclient.Get($"products/attributes/{attr_id}/terms");
                var anon = new[] { new { name = "" } };
                var arr = JsonConvert.DeserializeAnonymousType(some, anon);
                */

                var tmp = wooclient.GetAttributeTerms(attr_id).GetAwaiter().GetResult();
                var anon = new[] { new { name = "" } };
                var arr = JsonConvert.DeserializeAnonymousType(JsonConvert.SerializeObject(tmp), anon);



                terms[attr_id] = arr.Select(x => x.name).ToArray();
            }
            if (!terms[attr_id].Contains(term)) {

                /*
                some = wooclient.Post($"products/attributes/{attr_id}/terms", $@"{{""name"": ""{term}""}}");
                */
                wooclient.PostAttributeTerm(attr_id, term);
            }
            return term;
        }
    }
}