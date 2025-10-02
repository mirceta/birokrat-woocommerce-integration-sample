using biro_to_woo.loop;
using BirokratNext;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.tools.birokratops;
using core.tools.wooops;
using core.tools.zalogaretriever;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biro_to_woo.logic.change_trackers.exhaustive {
    public class ExhaustivePriceStockArtikelChangeTracker {

        BirokratField skuToBirokrat;
        IOutApiClient woo;
        IApiClientV2 biro;
        ILogger logger;
        IOutProductRetriever outProductRetriever;
        IBirokratArtikelRetriever birokratArtikelRetriever;
        bool verbose;
        bool addproducts_notonwebshop;
        bool include_articles_internetne;

        public ExhaustivePriceStockArtikelChangeTracker(IOutApiClient woo,
            IApiClientV2 biro,
            BirokratField skuToBirokrat,
            IBirokratArtikelRetriever birokratArtikelRetriever,
            ILogger logger,
            IOutProductRetriever productRetriever,
            bool addproducts_notonwebshop,
            bool include_articles_internetne,
            bool verbose = true) {

            this.skuToBirokrat = skuToBirokrat;
            this.woo = woo;
            this.biro = biro;
            this.logger = logger;
            this.outProductRetriever = productRetriever;
            this.verbose = verbose;
            this.birokratArtikelRetriever = birokratArtikelRetriever;
            this.addproducts_notonwebshop = addproducts_notonwebshop;
            this.include_articles_internetne = include_articles_internetne;
        }

        public async Task<List<string>> GetNewChanges() {

            var products = outProductRetriever.Get(woo);
            var artikli = await birokratArtikelRetriever.Query(null, null);
            
            if (!include_articles_internetne) {
                string internetFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.Internet);
                artikli = artikli.Where(x => {
                    return (string)x[internetFieldName] == "-1";
                }).ToList();
            }

            HashSet<string> sifrasDiff = DetectChanges(products, artikli);
            return sifrasDiff.ToList();
        }

        private HashSet<string> DetectChanges(List<Dictionary<string, object>> products, List<Dictionary<string, object>> artikli) {


            HashSet<string> sifrasDiff = new HashSet<string>();
            string sifraFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla);
            string skuFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(skuToBirokrat);

            foreach (var artikel in artikli) {
                bool found = false;
                foreach (var product in products) {
                    if (string.IsNullOrEmpty((string)artikel[skuFieldName]) ||
                        string.IsNullOrEmpty((string)product["sku"]))
                        continue;
                    if ((string)artikel[skuFieldName] == (string)product["sku"]) {
                        found = true;
                        AddOnPriceChange(sifrasDiff, artikel, product);
                        AddOnZalogaChange(sifrasDiff, artikel, product);
                        break;
                    }
                }
                if (!found && addproducts_notonwebshop) {
                    sifrasDiff.Add(((string)artikel[sifraFieldName]).Trim());
                    ConsolePrintout($"sifra: {(string)artikel[sifraFieldName]} not yet on external");
                }
            }

            return sifrasDiff;
        }

        public void Clear() {
            // do nothing
        }

        private void AddOnPriceChange(HashSet<string> sifrasDiff, Dictionary<string, object> artikel, Dictionary<string, object> product) {

            string sifraFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla);
            string priceFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.PCsPD);

            string wooprice = GWooOps.SerializeDblWooProperty(product["regular_price"]);
            if (GWooOps.SerializeDblWooProperty(product["regular_price"]) == "0") {
                wooprice = GWooOps.SerializeDblWooProperty(product["price"]);
            }

            double biroprice = Tools.ParseDoubleBigBrainTime((string)artikel[priceFieldName]);
            double woopric = Tools.ParseDoubleBigBrainTime(wooprice);

            if (Math.Abs(biroprice - woopric) > 0.02) {
                ConsolePrintout($"sifra: {(string)artikel[sifraFieldName]} sku: {(string)product["sku"]} cena biro: {(string)artikel[priceFieldName]} woo: {wooprice}");
                sifrasDiff.Add(((string)artikel[sifraFieldName]).Trim());
            }
        }

        private void AddOnZalogaChange(HashSet<string> sifrasDiff, Dictionary<string, object> artikel, Dictionary<string, object> product) {

            string sifraFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla);

            if (Tools.ParseDoubleBigBrainTime((string)artikel["zaloga"]) != Tools.ParseDoubleBigBrainTime(GWooOps.SerializeIntWooProperty(product["stock_quantity"]))) {
                ConsolePrintout($"sifra: {(string)artikel[sifraFieldName]} woosku: {(string)product["sku"]} zaloga biro: {(string)artikel["zaloga"]} woo: {GWooOps.SerializeIntWooProperty(product["stock_quantity"])}");
                sifrasDiff.Add(((string)artikel[sifraFieldName]).Trim());
            }
        }

        private void ConsolePrintout(string content) {
            if (verbose) {
                Console.WriteLine(content);
            }
        }
    }
}
