using BirokratNext;
using BirokratNext.api_clientv2;
using BirokratNext.Exceptions;
using BironextWordpressIntegrationHub;
using birowoo_exceptions;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.structs_wc_to_biro;
using BiroWooHub.logic.integration;
using core.tools.zalogaretriever;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.tools.birokratops
{
    public static class GBirokratOps
    {

        public static async Task<SimplejsonOrder> CreatePrevzem_ReturnJson(List<string> sifre, IApiClientV2 client) {
            var specs = new List<BirokratPostavka>();

            foreach (string tmp in sifre) {
                specs.Add(new BirokratPostavka() {
                    BirokratSifra = tmp,
                    Quantity = 50,
                    DiscountPercent = 0,
                    Subtotal = "30",
                });
            }

            var x = new SimplejsonOrder() {
                ExternalIdentifier = "sogsdfgmhgfe",
                DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.000000"),
                Billing = new MBilling() {
                    BirokratId = "0005",
                    Name = "Kristijan Mirceta",
                    Company = "Birokrat d.o.o.",
                    Email = "kristijan.mirceta@gmail.com",
                    Country = "SI",
                    City = "Ljubljana",
                    Postcode = "1000",
                    Address = "dunajska cesta 191",
                },
                Shipping = new MShipping() {
                    // lahko je drug partner kot pri billingu, izpolnjen na isti nacin.
                    Name = "Kristijan Mirceta",
                    Company = "Birokrat d.o.o.",
                    Country = "SI",
                    City = "Ljubljana",
                    Postcode = "1000",
                    Address = "dunajska cesta 191",
                },
                Specifications = specs
            };

            var some = JsonConvert.SerializeObject(x);
            string chome = await client.document.CreateSimpleJson("skladisce/prevzem/vnosinpregled", some);

            return x;
        }

        public static async Task<List<string>> GetChangedArticlesInLastHour(IApiClientV2 client, bool filterinternet, bool filteroutneuporabljaj) {
            while (true) {
                try {
                    var pars = await client.cumulative.Parametri("sifranti/artikli/podrobnipregledprodajnihartiklovinstoritev");
                    Dictionary<string, object> postback = pars.ToDictionary(x => x.Koda, y => y.PrivzetaVrednost);

                    DateTime now = DateTime.Now;
                    string datum = now.ToString("yyyy-MM-dd");
                    int ura = Math.Min(24, now.Hour + 1);
                    if (ura == 0) ura = 2;

                    postback["SpremembaOdDatuma"] = datum;
                    postback["SpremembaOdUre"] = (ura - 2).ToString();
                    postback["SpremembaDoUre"] = ura.ToString();
                    var data = await client.cumulative.Podatki("sifranti/artikli/podrobnipregledprodajnihartiklovinstoritev", postback);

                    // filtriraj se - da mora imet prenesi na internet kljukico in da ne uporabljaj ni false!
                    if (filterinternet) {
                        data = data.Where(x => {
                            if (x.ContainsKey("Prenesi v e-shop")) {
                                return (string)x["Prenesi v e-shop"] == "-1";
                            } else {
                                Console.WriteLine("");
                                throw new Exception("");
                            }
                        }).ToList();
                    }
                    if (filteroutneuporabljaj) {
                        data = data.Where(x => (string)x["Ne uporabljaj"] == "0" || string.IsNullOrEmpty((string)x["Ne uporabljaj"])).ToList();
                    }

                    return data.Select(x => (string)x["Artikel"]).ToList();
                } catch (BironextRestartException ex) {

                }
            }
            
        }

        public static async Task<List<string>> GetChangedArticlesSince(IApiClientV2 client, DateTime since, bool filterinternet, bool filteroutneuporabljaj) {

            while (true) {
                try {
                    var pars = await client.cumulative.Parametri("sifranti/artikli/podrobnipregledprodajnihartiklovinstoritev");
                    Dictionary<string, object> postback = pars.ToDictionary(x => x.Koda, y => y.PrivzetaVrednost);

                    List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                    if (since.DayOfYear != DateTime.Now.DayOfYear) {
                        var timesince = DateTime.Now.Subtract(since);
                        for (int i = 0; i < timesince.Days; i++) {
                            postback["SpremembaOdDatuma"] = since.Add(new TimeSpan(i, 0, 0, 0)).ToString("yyyy-MM-dd");
                            postback["SpremembaOdUre"] = (0).ToString();
                            postback["SpremembaDoUre"] = 24.ToString();
                            var data1 = await client.cumulative.Podatki("sifranti/artikli/podrobnipregledprodajnihartiklovinstoritev", postback);
                            data.AddRange(data1);
                        }
                    } else {
                        postback["SpremembaOdDatuma"] = DateTime.Now.ToString("yyyy-MM-dd");
                        postback["SpremembaOdUre"] = 0.ToString();
                        postback["SpremembaDoUre"] = 24.ToString();
                        data = await client.cumulative.Podatki("sifranti/artikli/podrobnipregledprodajnihartiklovinstoritev", postback);
                    }

                    // filtriraj se - da mora imet prenesi na internet kljukico in da ne uporabljaj ni false!
                    if (filterinternet) {
                        data = data.Where(x => (string)x["Prenesi v e-shop"] == "-1").ToList();
                    }
                    if (filteroutneuporabljaj) {
                        data = data.Where(x => (string)x["Ne uporabljaj"] == "0" || (string)x["Ne uporabljaj"] == "").ToList();
                    }

                    return data.Select(x => (string)x["Artikel"]).ToList();
                } catch (BironextRestartException ex) {

                }
            }
        }

        public static async Task<Dictionary<string, object>> GetAndBuildBirokratArtikel(IApiClientV2 client, IZalogaRetriever zaloga, string sifra) {
            string sifraonlyforupdateparameterss = sifra.Replace("/", "(slash)");
            var x = client.sifrant.UpdateParameters(@"sifranti\artikli\prodajniartikli-storitve", sifraonlyforupdateparameterss).GetAwaiter().GetResult();
            var biroArtikel = x
                .GroupBy(y => y.Koda)
                .Where(y => y.Count() == 1)
                .ToDictionary(y => y.Key, z => z.Last().PrivzetaVrednost);

            try {
                string zaloga1 = zaloga.Get(sifra);
                biroArtikel["zaloga"] = zaloga1;
            } catch (ArtikelNotFoundInStanjeZaloge ex) {
                biroArtikel["zaloga"] = "0";
                Console.WriteLine($"Zaloga was not found for {sifra}");
            }
            return biroArtikel;
        }
        
        public static async Task<List<Dictionary<string, object>>> CurrentZaloga(IApiClientV2 client) {
            while (true) {
                try {
                    var res = client.cumulative.Parametri("sifranti/artikli/stanjezaloge").GetAwaiter().GetResult();
                    var stanjeZalogeParams = res
                        .GroupBy(x => x.Koda)
                        .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
                    stanjeZalogeParams["Artikel"] = "";
                    stanjeZalogeParams["ArtikelOpis"] = "";
                    stanjeZalogeParams["Centralno"] = true;
                    stanjeZalogeParams["MP"] = true;
                    stanjeZalogeParams["MP1"] = true;
                    stanjeZalogeParams["MP2"] = true;
                    stanjeZalogeParams["MP3"] = true;
                    stanjeZalogeParams["MP4"] = true;
                    stanjeZalogeParams["MP5"] = true;
                    var zaloga = await client.cumulative.Podatki("sifranti/artikli/stanjezaloge", stanjeZalogeParams);
                    return zaloga;
                } catch (BironextRestartException ex) {

                }
            }
        }

        public static List<string> GetSifrasWhereZalogaDiff(List<Dictionary<string, object>> z1, List<Dictionary<string, object>> z2) {
            try {
                var x1 = z1.ToDictionary(x => x["Artikel"], y => y);
                var x2 = z2.ToDictionary(x => x["Artikel"], y => y);

                List<string> sifreDiff = new List<string>();
                foreach (string sifraArtikla in x1.Keys) {
                    if (x2.ContainsKey(sifraArtikla)) {
                        if ((string)x1[sifraArtikla]["Skupna Zaloga"] != (string)x2[sifraArtikla]["Skupna Zaloga"]) {
                            sifreDiff.Add(sifraArtikla);
                        }
                    }
                }

                return sifreDiff;
            } catch (Exception ex) {
                string msg = "Zaloga1: " + JsonConvert.SerializeObject(z1) + "\n Zaloga2: " + JsonConvert.SerializeObject(z2);
                throw new Exception(msg, ex);
            }
        }

        public static async Task GetAndSavePdf(IApiClientV2 bironext, string apipath, string stevilkaDokumenta, string desiredName, string folder = "") {
            string pdf = await bironext.document.GetPdf(apipath, stevilkaDokumenta);
            var ano = new { content = "" };
            string base64 = JsonConvert.DeserializeAnonymousType(pdf, ano).content;
            await Tools.SavePdf(base64, desiredName, folder);
        }

        public static async Task<string> GetPdf(IApiClientV2 bironext, string apipath, string stevilkaDokumenta)
        {
            string pdf = await bironext.document.GetPdf(apipath, stevilkaDokumenta);
            var ano = new { content = "" };
            string base64 = JsonConvert.DeserializeAnonymousType(pdf, ano).content;
            return base64;
        }
    }
}
