using BirokratNext;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{
    public class PartnerSearchHelper {
        public async static Task<List<Dictionary<string, object>>> findmatches(IApiClientV2 client, 
                string searchterm, 
                Func<Dictionary<string, object>, bool> comparison) {
            string sifrantPartnerjevPath = @"sifranti\poslovnipartnerjiinosebe\poslovnipartnerji";
            var matches = await client.sifrant.Podatki(sifrantPartnerjevPath, searchterm);
            var truematches = new List<Dictionary<string, object>>();
            foreach (var x in matches) {

                if (comparison(x)) {
                    truematches.Add(x);
                }

            }
            return truematches;
        }

        public async static Task<List<Dictionary<string, object>>> findByDavcna(IApiClientV2 client,
                string davcna,
                Func<Dictionary<string, object>, bool> comparison)
        {
            var pars = await client.cumulative.Parametri("sifranti/poslovnipartnerjiinosebe/podrobnipregledpartnerjev");
            Dictionary<string, object> postback = pars.ToDictionary(x => x.Koda, y => y.PrivzetaVrednost);
            var some = JsonConvert.SerializeObject(postback);
            postback["DavcnaStevilka"] = davcna;
            postback["NimaDavcneImaTRR"] = false;
            var matches = await client.cumulative.Podatki("sifranti/poslovnipartnerjiinosebe/podrobnipregledpartnerjev", postback);

            var truematches = new List<Dictionary<string, object>>();
            some = JsonConvert.SerializeObject(matches);
            foreach (var x in matches)
            {

                if (comparison(x))
                {
                    truematches.Add(x);
                }

            }
            return truematches;
        }
    }
}
