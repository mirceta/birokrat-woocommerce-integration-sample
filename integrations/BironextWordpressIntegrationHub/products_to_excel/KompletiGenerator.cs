using Aspose.Cells;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.logic;
using BiroWooHub.logic.integration;
using core.tools.wooops;
using Newtonsoft.Json;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace products_to_excel
{
    public class KompletiGenerator {

        /*
         From webshop retrieve articles of format XXXX/YY-ZZZZ/AA-BBBB/CC
        These are compilations of products.

        Out of this you create the articles for each possible combination of these,
        then artikli sestava linking them back to the originals!
         */

        IIntegration integration;
        bool cached;
        ISourceDataRetriever sourceDataRetriever;
        IMyLogger logger;
        public KompletiGenerator(IIntegration integration, 
            bool cached,
            IMyLogger logger)
        {
            this.integration = integration;
            this.logger = logger;
            this.sourceDataRetriever = new SourceDataRetriever();
            if (cached)
            {
                sourceDataRetriever = new Cached((SourceDataRetriever)sourceDataRetriever);
                logger.LogInformation("POZOR! Uporabljas shranjevanje podatkov! Ob posodobitvi podatkov bo program se zmeraj uporabljal stare podatke!");
            }
            else
                Cached.KillCache();
            
        }

        const string sestava_filename = "artikli_sestava.xls";
        const string artikli_kompleti_filename = "artikli_kompleti.xls";

        public async Task Execute() {

            try
            {
                logger.LogInformation("Zacenjam izvajanje...");
                var data = sourceDataRetriever.GetSourceData(integration);
                List<Dictionary<string, object>> tmp = FilterSets(data);
                logger.LogInformation($"Nasel sem {tmp.Count} kompletov...");

                List<BiroSetArtikel> all = ToArtikliWithSestava(data, tmp);
                logger.LogInformation($"Ustvaril sem {all.Count} kombinacij...");

                await Create_Artikli_Excel_FromImportIntoBirokrat(all);
                logger.LogInformation($"Ustvaril sem datoteko za uvoz novih artiklov v {Path.Combine(Directory.GetCurrentDirectory(), artikli_kompleti_filename)}");
                await Create_ArtikliSestava_Excel_ForImportIntoBirokrat(all);
                logger.LogInformation($"Ustvaril sem datoteko za uvoz sestave artiklov v {Path.Combine(Directory.GetCurrentDirectory(), sestava_filename)}");
            }
            catch (Exception ex) {
                logger.LogInformation($"Izvajanje neuspesno: {ex.Message} {ex.StackTrace.ToString()}");
            }
        }

        private async Task Create_ArtikliSestava_Excel_ForImportIntoBirokrat(List<BiroSetArtikel> all) {

            Workbook wb = new Workbook();
            wb.Worksheets.Add(SheetType.Worksheet);
            var ws = wb.Worksheets[0];

            ws.Cells[0, 0].Value = "XXXX";
            int row_index = 1;

            foreach (var art in all) {

                int SIFRA_PROD_ART = 0;
                int SIFRA_NAB_ART = 1;
                int OPIS = 3;
                int KOLICINA = 8;
                int SKUPINA = 11;


                string hash = Tools.GetHashCode(art.sifra);

                ws.Cells[row_index, SIFRA_PROD_ART].Value = hash;
                ws.Cells[row_index, OPIS].Value = art.name;
                ws.Cells[row_index, KOLICINA].Value = 1;
                ws.Cells[row_index, SKUPINA].Value = "PRODAJNI";
                row_index++;

                foreach (var orig in art.original_item_sifras) {
                    ws.Cells[row_index, SIFRA_NAB_ART].Value = orig;
                    ws.Cells[row_index, SIFRA_PROD_ART].Value = hash;
                    ws.Cells[row_index, KOLICINA].Value = 1;
                    ws.Cells[row_index, SKUPINA].Value = "NABAVNI";
                    row_index++;
                }
            }

            wb.Save(sestava_filename);
        }

        private async Task Create_Artikli_Excel_FromImportIntoBirokrat(List<BiroSetArtikel> all) {
            Workbook wb = new Workbook();
            wb.Worksheets.Add(SheetType.Worksheet);
            var ws = wb.Worksheets[0];


            ws.Cells[0, 0].Value = "XXXX";

            int row_index = 1;

            foreach (var art in all) {

                int SIFRA_PROD_ART = 0;
                int BARKODA5 = 34;
                int OPIS = 2;
                int ENOTA = 6;
                int SIFRA_DAVKA = 5;

                string hash = Tools.GetHashCode(art.sifra);

                ws.Cells[row_index, SIFRA_PROD_ART].Value = hash;
                ws.Cells[row_index, BARKODA5].Value = art.sifra;
                ws.Cells[row_index, ENOTA].Value = "kos";
                ws.Cells[row_index, OPIS].Value = art.name;
                ws.Cells[row_index, SIFRA_DAVKA].Value = 1;
                row_index++;
            }

            wb.Save(artikli_kompleti_filename);
        }

        private async Task CreateRootArticlesWithApi(List<BiroSetArtikel> all, IIntegration integ) {
            foreach (var prod in all) {

                string hash = "";
                var argses = new SearchThenIfNotFoundCreateArgs() {
                    sifrantpath = @"sifranti/artikli/prodajniartikli-storitve",
                    searchterm = prod.sifra,
                    nameoffieldtocomparewith = "Šifra artikla",
                    valuetocomparewith = prod.sifra,
                    pack = new Dictionary<string, object>() {
                    { "txtOpis", prod.name },
                    { "txtSifraArtikla", prod.sifra},
                    { "Barkoda5", prod.sifra},
                    { "SifraDavka", "1    22 DDV osnovna stopnja" },
                    { "txtEnota", "kos"}
                },
                    fieldtoreturn = "Šifra artikla"
                };
                await new ClassicBirokratSifrantPersistor(integ.BiroClient).SearchThenIfNotFoundCreate(argses);
            }
        }

        private List<BiroSetArtikel> ToArtikliWithSestava(List<Dictionary<string, object>> data, List<Dictionary<string, object>> tmp) {
            List<BiroSetArtikel> all = new List<BiroSetArtikel>();
            foreach (var product in tmp) {
                string origi_sku = (string)product["sku"];
                List<BiroSetArtikel> sestava = GetOriginalArticles(data, product, origi_sku);
                all.AddRange(sestava);
            }

            return all;
        }

        private List<BiroSetArtikel> GetOriginalArticles(List<Dictionary<string, object>> data, Dictionary<string, object> product, string origi_sku) {
            var variations = data.Where(x =>
                            ((x.ContainsKey("parent_id") && GWooOps.SerializeIntWooProperty(x["parent_id"]) == GWooOps.SerializeIntWooProperty(product["id"]))) ||
                                             (x.ContainsKey("original_id") && GWooOps.SerializeIntWooProperty(x["original_id"]) == GWooOps.SerializeIntWooProperty(product["id"])))
                                           .ToList()
                                           .Select(x => {

                                               // attribute choices
                                               var attr = x["attributes"];
                                               var some = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(JsonConvert.SerializeObject(attr));
                                               var choices = some.Select(x => ((string)x["option"]).ToUpper()).ToList();

                                               string[] parts = origi_sku.Split("/");
                                               var original_products = parts.Zip(choices, (x, y) => x + "/" + y).ToList();

                                               var set_sku = original_products.Aggregate("", (x, y) => x + "-" + y).Substring(1);

                                               string name = (string)product["name"];

                                               return new BiroSetArtikel() { name = name, sifra = set_sku, original_item_sifras = original_products };
                                           }).ToList();
            return variations;
        }

        private List<Dictionary<string, object>> FilterSets(List<Dictionary<string, object>> data) {
            return data.Where(x =>
                                ((x.ContainsKey("parent_id") && GWooOps.SerializeIntWooProperty(x["parent_id"]) == "0")) ||
                                 (x.ContainsKey("original_id") && GWooOps.SerializeIntWooProperty(x["original_id"]) == "0"))
                        .Where(x => {
                            string sku = ((string)x["sku"]);
                            string[] parts = sku.Split("/");
                            if (parts.Length >= 2 && parts.All(x => x.Length >= 2)) {
                                return true;
                            }
                            return false;
                        })
                .ToList();
        }
    }
}
