using BirokratNext;
using birowoo_exceptions;
using BiroWoocommerceHub.flows;
using core.logic.common_birokrat;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.tools.zalogaretriever
{
    public class PerPartesZalogaRetriever : IZalogaRetriever
    {

        IApiClientV2 client;
        Dictionary<string, string> zalogaNameMap;
        bool popraviZaSestavljeneArtikle;
        /*zalogaNameMap: for all prodajno mesto zaloga map its name as seen in the kumulativa PL parameters to
          how the column header is in kumulativa data.
          for example: MP1 -> Ljubljana, MP2 -> Celje, Centralno -> Centralno


         Will sum up all prodajna mesta in zalogaNameMap
         */
        public PerPartesZalogaRetriever(IApiClientV2 client, Dictionary<string, string> zalogaNameMap, bool popraviZaSestavljeneArtikle = false) {
            this.client = client;
            this.zalogaNameMap = zalogaNameMap;
            this.popraviZaSestavljeneArtikle = popraviZaSestavljeneArtikle;
        }

        public Dictionary<string, object> Infer(Dictionary<string, object> state)
        {
            state["zalogaRetriever"] = this;
            return state;
        }

        public  string Get(string sifra) {

            if (zalogaSSestavo != null && zalogaSSestavo.ContainsKey(sifra))
                return zalogaSSestavo[sifra];

            var res = client.cumulative.Parametri("sifranti/artikli/stanjezaloge").GetAwaiter().GetResult();
            var stanjeZalogeParams = res
                .GroupBy(x => x.Koda)
                .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            stanjeZalogeParams["SifraArtikla"] = sifra;
            stanjeZalogeParams["ArtikelOpis"] = "";
            stanjeZalogeParams["ArtikliVsi"] = true;
            stanjeZalogeParams["ArtikliZStanjem"] = false;
            stanjeZalogeParams["ArtikliPozitivni"] = false;
            stanjeZalogeParams["ArtikliNegativni"] = false;
            stanjeZalogeParams["ArtikliPodMinimalnoZalogo"] = false;

            foreach (var key in zalogaNameMap.Keys) {
                stanjeZalogeParams[key] = true;
            }
            
            var stanjeZalogeData = client.cumulative.Podatki("sifranti/artikli/stanjezaloge", stanjeZalogeParams).GetAwaiter().GetResult();
           

            string a1 = BirokratNameOfFieldInFunctionality.KumulativaStanjeZaloga(BirokratField.SifraArtikla);
            string zaloga = "";
            bool matchfound = false;
            foreach (var entry in stanjeZalogeData) {
                if (((string)entry[a1]) == sifra) {
                    zaloga = GetZalogaForEntry(entry);
                    matchfound = true;
                    break;
                }
            }
            if (!matchfound)
                throw new ArtikelNotFoundInStanjeZaloge(sifra);
            return zaloga;
        }

        public async Task<List<Tuple<string, string>>> Query() {
            List<Tuple<string, string>> retval = await GetWholeZaloga();
            if (popraviZaSestavljeneArtikle) {
                retval = await PopraviZalogoSSestavo(retval);
            }
            return retval;
        }

        private async Task<List<Tuple<string, string>>> GetWholeZaloga() {
            var res = client.cumulative.Parametri("sifranti/artikli/stanjezaloge").GetAwaiter().GetResult();
            var stanjeZalogeParams = res
                .GroupBy(x => x.Koda)
                .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            stanjeZalogeParams["SifraArtikla"] = "";
            stanjeZalogeParams["ArtikelOpis"] = "";
            stanjeZalogeParams["ArtikliVsi"] = true;
            stanjeZalogeParams["ArtikliZStanjem"] = false;
            stanjeZalogeParams["ArtikliPozitivni"] = false;
            stanjeZalogeParams["ArtikliNegativni"] = false;
            stanjeZalogeParams["ArtikliPodMinimalnoZalogo"] = false;



            foreach (var key in zalogaNameMap.Keys) {
                stanjeZalogeParams[key] = true;
            }

            var zaloga = await client.cumulative.Podatki("sifranti/artikli/stanjezaloge", stanjeZalogeParams);

            List<Tuple<string, string>> retval = new List<Tuple<string, string>>();
            foreach (var entry in zaloga) {
                var some = GetZalogaForEntry(entry);
                string a1 = BirokratNameOfFieldInFunctionality.KumulativaStanjeZaloga(BirokratField.SifraArtikla);
                retval.Add(Tuple.Create<string, string>(((string)entry[a1]).Trim(), some));
            }

            return retval;
        }


        Dictionary<string, string> zalogaSSestavo = null;
        async Task<List<Tuple<string, string>>> PopraviZalogoSSestavo(List<Tuple<string, string>> zaloga) {
            var sestava = await client.cumulative.Podatki("sifranti/artikli/sestavezarazknjizevanjezaloge");

            var boundaryIndices = new List<int>();
            for (int i = 1; i < sestava.Count; i++) {
                var prev = sestava[i - 1];
                var cur = sestava[i];
                if (!string.IsNullOrEmpty((string)prev["0"]) && // "0" right now is SifraProdajni!
                    string.IsNullOrEmpty((string)cur["0"]))
                    boundaryIndices.Add(i - 1);
            }

            var dic = zaloga.ToDictionary(x => x.Item1, x => x.Item2);
            zalogaSSestavo = new Dictionary<string, string>();
            foreach (var idx in boundaryIndices) {

                // origi
                string prodSifra = (string)sestava[idx]["0"];

                // components
                int offset = 0;
                List<Tuple<string, string>> lstNabSifra_Kolicina = new List<Tuple<string,string>>();
                do {
                    string nabavnaSifra = (string)sestava[idx + offset]["3"];
                    string kolicina = (string)sestava[idx + offset]["Količina v sestavi"];
                    lstNabSifra_Kolicina.Add(Tuple.Create(nabavnaSifra, kolicina));
                    offset++;
                } while (string.IsNullOrEmpty((string)sestava[idx + offset]["0"]));

                // calc zaloga
                List<int> accum = new List<int>();
                foreach (var nabavni in lstNabSifra_Kolicina) {
                    int kol = int.Parse(dic[nabavni.Item1]);
                    int zahteva = int.Parse(nabavni.Item2);
                    accum.Add(kol / zahteva);
                }

                int storage = accum.Min();
                dic[prodSifra] = storage + "";
                zalogaSSestavo[prodSifra] = storage + "";
            }

            return dic.ToList().Select(x => Tuple.Create(x.Key, x.Value)).ToList();
        }

        private string GetZalogaForEntry(Dictionary<string, object> entry) {
            string zaloga;
            int skupnaZaloga = 0;
            foreach (var value in zalogaNameMap.Values) {

                if (!entry.ContainsKey(value)) {
                    throw new IntegrationProcessingException($"Stanje zaloge ni vsebovalo polja {value}");
                }

                string tmp = (string)entry[value];
                if (!string.IsNullOrEmpty(tmp)) {
                    int some = 0;
                    int.TryParse(tmp, out some);
                    skupnaZaloga += some;
                }
            }

            zaloga = "" + skupnaZaloga;
            return zaloga;
        }
    }
}
