using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.logic;
using BiroWoocommerceHub.structs_wc_to_biro;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion.postavke_additions
{
    public class ProvizijaZaOdkupnino_PostavkaAddOp : IAdditionalOperationOnPostavke
    {
        IApiClientV2 client;
        string SifraDavka = "";
        string provizija = "0";
        public ProvizijaZaOdkupnino_PostavkaAddOp(IApiClientV2 client, string provizija, string SifraDavka = "1    22 DDV osnovna stopnja") {
            this.client = client;
            this.SifraDavka = SifraDavka;
            this.provizija = provizija;
        }
        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke) {


            provizija = provizija.Replace(",", ".");
            var postavka = new BirokratPostavka() {
                BirokratSifra = "",
                Quantity = 1,
                Subtotal = provizija,
            };

            string shippingmethod = "Provizija za odkupnino";
            string hash = Tools.GetHashCode(shippingmethod) + "1";
            if (order.Data.PaymentMethod == "cod") {
                var args = new SearchThenIfNotFoundCreateArgs() {
                    sifrantpath = @"sifranti/artikli/prodajniartikli-storitve",
                    searchterm = hash,
                    nameoffieldtocomparewith = "txtSifraArtikla",
                    valuetocomparewith = hash,
                    pack = new Dictionary<string, object>() {
                        { "txtOpis", shippingmethod },
                        { "txtSifraArtikla", hash},
                        { "SifraDavka", SifraDavka },
                        { "txtEnota", "kos"}
                    },
                    fieldtoreturn = "txtSifraArtikla"
                };
                string sifra = await new ClassicBirokratSifrantPersistor(client, null, new BirokratArtikelRetrieverSearchStrategy(client)).SearchThenIfNotFoundCreate(args);
                postavka.BirokratSifra = sifra;
                postavke.Add(postavka);
            }
            return postavke;
        }
    }
}
