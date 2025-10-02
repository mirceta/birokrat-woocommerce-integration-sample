using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.logic;
using BiroWoocommerceHub.structs_wc_to_biro;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion.postavke_additions
{
    public class Shipping_PostavkaAddOp : IAdditionalOperationOnPostavke
    {
        IApiClientV2 client;
        string SifraDavka = "";
        List<IAdditionalOperationOnPostavke> operationsOnlyOnShipping;
        string sifraAddition;
        public Shipping_PostavkaAddOp(IApiClientV2 client, 
            string SifraDavka = "4    22 DDV osnovna stopnja            Storitev", 
            List<IAdditionalOperationOnPostavke> operationsOnlyOnShipping = null,
            string sifraAddition = "") {
            this.client = client;
            this.SifraDavka = SifraDavka;
            this.operationsOnlyOnShipping = operationsOnlyOnShipping;
            this.sifraAddition = sifraAddition;
        }
        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke)
        {
            if (Tools.ParseDoubleBigBrainTime(order.Data.ShippingTotal) > 0) {

                string shippingmethod = order.ShippingMethod;
                if (string.IsNullOrEmpty(shippingmethod)) {
                    shippingmethod = "Poštnina";
                }
                string hash = Tools.GetHashCode(shippingmethod + sifraAddition);

                string shippingAmount = "" + Math.Round(100.0 * (Tools.ParseDoubleBigBrainTime(order.Data.ShippingTotal) + Tools.ParseDoubleBigBrainTime(order.Data.ShippingTax))) / 100.0;
                shippingAmount = shippingAmount.Replace(",", ".");
                var postavka = new BirokratPostavka() {
                    BirokratSifra = "",
                    Quantity = 1,
                    Subtotal = shippingAmount,
                };

                var args = new SearchThenIfNotFoundCreateArgs() {
                    sifrantpath = @"sifranti/artikli/prodajniartikli-storitve",
                    searchterm = hash,
                    nameoffieldtocomparewith = "txtSifraArtikla",
                    valuetocomparewith = hash,
                    pack = new Dictionary<string, object>() {
                        { "txtOpis", shippingmethod },
                        { "txtSifraArtikla", hash},
                        { "SifraDavka", SifraDavka },
                        { "txtEnota", "storitev"}
                    },
                    fieldtoreturn = "txtSifraArtikla"
                };
                string sifra = await new ClassicBirokratSifrantPersistor(client, null, new BirokratArtikelRetrieverSearchStrategy(client)).SearchThenIfNotFoundCreate(args);

                postavka.BirokratSifra = sifra;
                postavka = await ApplyNeededOpsToShipping(order, postavka);

                postavke.Add(postavka);
            }
            return postavke;
        }

        private async Task<BirokratPostavka> ApplyNeededOpsToShipping(WoocommerceOrder order, BirokratPostavka postavka) {

            if (operationsOnlyOnShipping == null)
                return postavka;

            foreach (var op in operationsOnlyOnShipping) {
                postavka = (await op.ApplyOperationToPostavke(order, new List<BirokratPostavka>() { postavka }))[0];
            }

            return postavka;
        }
    }
}
