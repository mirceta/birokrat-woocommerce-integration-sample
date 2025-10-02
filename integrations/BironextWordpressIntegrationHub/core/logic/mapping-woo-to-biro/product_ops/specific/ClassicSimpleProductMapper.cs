using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.logic;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.logic.mapping_woo_to_biro;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.customers.poledancerka.mappers
{
    public class ClassicSimpleProductMapper : IWooToBiroProductMapper
    {
        IApiClientV2 client;
        string sifraDavkaZaNeobstojecIzdelek;
        bool putSkuIntoComment = false;
        BirokratPostavkaUtils utils;
        public ClassicSimpleProductMapper(BirokratPostavkaUtils utils, IApiClientV2 client, bool putSkuIntoComment, string sifraDavkaZaNeobstojecIzdelek = "1    22 DDV osnovna stopnja") {
            this.client = client;
            this.sifraDavkaZaNeobstojecIzdelek = sifraDavkaZaNeobstojecIzdelek;
            this.putSkuIntoComment = putSkuIntoComment;
            this.utils = utils;
        }


        public string GetOrAddProductAndReturnSifra()
        {
            throw new NotImplementedException();
        }

        public bool IsThisTypeOfProduct(dynamic x)
        {
            return GWooOps.OriginProductAttributeCount(x) == 0;
        }

        public async Task MapWooProductToBirokrat(Dictionary<string, object> product)
        {
            string sku = ((string)product["sku"]);
            string search = sku;
            string opis = ((string)product["name"]);
            string sifra = await GetOrCreateBirokratItem(sku, search, opis);
        }

        public async Task<BirokratPostavka> ProductItemToBirokratPostavka(WoocommerceOrderItem x, bool VerifyAndCreate) {

            var p = utils.Get(x, x.BirokratSifra);

            string sku = ((string)x.OriginProduct["sku"]);
            if (string.IsNullOrEmpty(sku)) {
                throw new Exception($"For dancerka simple product mapper SKU code is required but was not found for product {x.Name}");
            }
            string search = sku;
            string opis = ((string)x.Name);

            string sifra = "";
            if (VerifyAndCreate) {
                sifra = await GetOrCreateBirokratItem(sku, search, opis);
            } else {
                sifra = sku;
            }
            if (putSkuIntoComment) {
                p.Comment = $"SKU: {sifra}";
            }
            p.BirokratSifra = sifra;
            return p;
        }

        private async Task<string> GetOrCreateBirokratItem(string sku, string search, string opis) {

            var args = new SearchThenIfNotFoundCreateArgs() {
                sifrantpath = @"sifranti/artikli/prodajniartikli-storitve",
                searchterm = search,
                nameoffieldtocomparewith = "txtSifraArtikla",
                valuetocomparewith = sku,
                fieldtoreturn = "txtSifraArtikla",
                pack = new Dictionary<string, object>() {
                                { "txtOpis", opis },
                                { "txtSifraArtikla", sku},
                                { "SifraDavka", sifraDavkaZaNeobstojecIzdelek },
                                { "txtEnota", "kos"}
                            },
            };

            string sifra = await new ClassicBirokratSifrantPersistor(client, null, new BirokratArtikelRetrieverSearchStrategy(client)).SearchThenIfNotFoundCreate(args);
            return sifra;
        }
    }
}
