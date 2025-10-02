using BiroWoocommerceHub.flows;
using BiroWooHub.logic.integration;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tests.tools
{
    public class TestUtils {
        public static string TestEqual(Dictionary<string, object> biro, string biroKey, Dictionary<string, object> woo, string wooKey, string type = "str")
        {

            string some = null;
            string chome = null;
            if (type == "str")
            {
                some = (string)biro[biroKey];
                chome = (string)woo[wooKey];

            }
            else if (type == "dbl")
            {
                some = Tools.ParseDoubleBigBrainTime((string)biro[biroKey]).ToString();
                chome = Tools.ParseDoubleBigBrainTime((string)woo[wooKey]).ToString();
            }
            else if (type == "int")
            {
                some = (string)biro[biroKey] == "" ? "0" : (string)biro[biroKey];
                chome = GWooOps.SerializeIntWooProperty(woo[wooKey]);
            }
            else if (type == "varattr")
            {
                some = (string)biro[biroKey];

                var tmp = woo["attributes"];
                var tmpjson = JsonConvert.SerializeObject(tmp);
                var tmpt = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(tmpjson);
                var tmpl = tmpt.Where(x => (string)x["name"] == wooKey).ToList();
                if (tmpl.Count == 1)
                {
                    chome = (string)tmpl[0]["option"];
                }
                else
                {
                    chome = "";
                }
            }
            else if (type == "varattrshopify")
            {

            }

            if (some != chome)
            {
                return $"FAIL Biro= {biroKey}:{some} Woo= {wooKey}:{chome}";
            }
            return $"PASS Biro= {biroKey}:{some} Woo= {wooKey}:{chome}";
        }

        public static async Task DecreasePriceForOneEuroInBirokrat_ThenVerify(IIntegration integ, string sifra)
        {
            var some = integ.BiroClient.sifrant.UpdateParameters(@"sifranti/artikli/prodajniartikli-storitve", sifra).GetAwaiter().GetResult();
            var biroArtikel = some
            .GroupBy(x => x.Koda)
            .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            double cena = Tools.ParseDoubleBigBrainTime((string)biroArtikel["PCsPD"]);
            cena = cena - 1;
            string chome = cena.ToString("##.#").Replace(".", ",");
            biroArtikel["PCsPD"] = chome;
            integ.BiroClient.sifrant.Update(@"sifranti/artikli/prodajniartikli-storitve", biroArtikel).GetAwaiter().GetResult();
            some = integ.BiroClient.sifrant.UpdateParameters(@"sifranti/artikli/prodajniartikli-storitve", sifra).GetAwaiter().GetResult();
            biroArtikel = some
            .GroupBy(x => x.Koda)
            .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            if (Tools.ParseDoubleBigBrainTime((string)biroArtikel["PCsPD"]) != Tools.ParseDoubleBigBrainTime(chome))
                throw new Exception("Birokrat did not update the price!");
        }

        public static async Task DecreasePriceInBirokrat_ThenVerify(IIntegration integ, string sifra, string new_price)
        {
            var some = integ.BiroClient.sifrant.UpdateParameters(@"sifranti/artikli/prodajniartikli-storitve", sifra).GetAwaiter().GetResult();
            var biroArtikel = some
            .GroupBy(x => x.Koda)
            .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            string chome = new_price.Replace(".", ",");
            biroArtikel["PCsPD"] = chome;
            integ.BiroClient.sifrant.Update(@"sifranti/artikli/prodajniartikli-storitve", biroArtikel).GetAwaiter().GetResult();
            some = integ.BiroClient.sifrant.UpdateParameters(@"sifranti/artikli/prodajniartikli-storitve", sifra).GetAwaiter().GetResult();
            biroArtikel = some
            .GroupBy(x => x.Koda)
            .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            if (Tools.ParseDoubleBigBrainTime((string)biroArtikel["PCsPD"]) != Tools.ParseDoubleBigBrainTime(chome))
                throw new Exception("Birokrat did not update the price!");
        }
    }
}
