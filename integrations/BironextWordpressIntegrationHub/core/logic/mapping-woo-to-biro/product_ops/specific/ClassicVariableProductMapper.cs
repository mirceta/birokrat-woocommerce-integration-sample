using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using birowoo_exceptions;
using BiroWoocommerceHub.logic;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.logic.common_exceptions;
using core.logic.mapping_woo_to_biro;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.customers.poledancerka.mappers
{

    public interface ISkuToSearch {
        public string Map(string sku);
    }

    public class RegularSkuToSearch : ISkuToSearch {
        public string Map(string sku) {
            return sku;
        }
    }
    public class PoledancerkaSkuToSearch : ISkuToSearch
    {
        public string Map(string sku) {
            string search = "";
            try {
                search = sku.Substring(0, sku.IndexOf("/"));
            } catch (Exception ex) {
                throw new IntegrationProcessingException($"Pri izdelku {sku}, sku koda ni pravilno formatirana");
            }
            return search;
        }
    }

    public class ClassicVariableProductMapper : IWooToBiroProductMapper
    {

        IApiClientV2 client;
        int lessThanHowManyVarAttributes;
        string sifraDavka;
        ISkuToSearch skuToSearch;
        bool addSkuToComment;
        BirokratPostavkaUtils utils;

        public ClassicVariableProductMapper(BirokratPostavkaUtils utils, IApiClientV2 client, bool addSkuToComment, int lessThanHowManyVarAttributes, ISkuToSearch skuToSearch, string sifraDavka = "1    22 DDV osnovna stopnja") {
            this.client = client;
            this.lessThanHowManyVarAttributes = lessThanHowManyVarAttributes;
            this.sifraDavka = sifraDavka;
            this.skuToSearch = skuToSearch;
            this.addSkuToComment = addSkuToComment;
            this.utils = utils;
        }


        public string GetOrAddProductAndReturnSifra()
        {
            throw new NotImplementedException();
        }

        public bool IsThisTypeOfProduct(dynamic x)
        {
            int cnt = GWooOps.OriginProductAttributeCount(x);
            return cnt > 0 && cnt < lessThanHowManyVarAttributes;
        }

        public async Task MapWooProductToBirokrat(Dictionary<string, object> product)
        {
            string sku = ((string)product["sku"]);
            string search = sku.Substring(0, sku.IndexOf("/"));
            string opis = ((string)product["name"]);
            string sifra = await GetOrCreateBirokratItem(sku, search, opis);
        }

        public async Task<BirokratPostavka> ProductItemToBirokratPostavka(WoocommerceOrderItem x, bool verifyAndCreate)
        {
            // PODPRTI SO SAMO VARIABILNI PRODUKTI!!!
            if (x.OriginProduct["variations"] == null)
                throw new Exception("This product is not variable!");

            var p = utils.Get(x, x.BirokratSifra);

            string sku = x.Sku;
            string search = skuToSearch.Map(sku);
            string opis = ((string)x.Name);
            
            string sifra = "";
            if (verifyAndCreate) {
                sifra = await GetOrCreateBirokratItem(sku, search, opis);
            } else {
                sifra = sku;
            }

            p.BirokratSifra = sifra;
            if (addSkuToComment)
                p.Comment = $"SKU: {sifra}";
            return p;
        }

        private async Task<string> GetOrCreateBirokratItem(string sku, string search, string opis) {
            
            var args = new SearchThenIfNotFoundCreateArgs()
            {
                sifrantpath = @"sifranti/artikli/prodajniartikli-storitve",
                searchterm = search,
                nameoffieldtocomparewith = "txtSifraArtikla",
                valuetocomparewith = sku,
                fieldtoreturn = "txtSifraArtikla",
                pack = new Dictionary<string, object>() {
                                { "txtOpis", opis },
                                { "txtSifraArtikla", sku}, 
                                { "SifraDavka", sifraDavka },
                                { "txtEnota", "kos"}
                            },
            };

            string sifra = await new ClassicBirokratSifrantPersistor(client, null, new BirokratArtikelRetrieverSearchStrategy(client)).SearchThenIfNotFoundCreate(args);
            return sifra;
        }
    }
}
