using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.logic;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.tools.wooops;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion.postavke_additions
{
    public class ConditionalAddExpense_PostavkaAddOp : IAdditionalOperationOnPostavke
    {
        IApiClientV2 client;
        IOrderAddOpCondition condition;
        string name;
        string price;
        string taxmethod;

        public ConditionalAddExpense_PostavkaAddOp(IApiClientV2 client, IOrderAddOpCondition condition, 
            string name,
            string price,
            string taxmethod) {
            this.client = client;
            this.condition = condition;
            this.name = name;
            this.price = price;
            this.taxmethod = taxmethod;
        }

        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke) {
            if (condition.Is(order)) {

                string sifra = "XOY0XOY";
                var postavka = new BirokratPostavka() {
                    BirokratSifra = sifra,
                    Quantity = 1,
                    Subtotal = price,
                };
                var args = new SearchThenIfNotFoundCreateArgs() {
                    sifrantpath = @"sifranti/artikli/prodajniartikli-storitve",
                    searchterm = sifra,
                    nameoffieldtocomparewith = "txtSifraArtikla",
                    valuetocomparewith = sifra,
                    pack = new Dictionary<string, object>() {
                        { "txtOpis", name },
                        { "txtSifraArtikla", sifra},
                        { "SifraDavka", taxmethod },
                        { "txtEnota", "storitev"}
                    },
                    fieldtoreturn = "txtSifraArtikla"
                };
                
                sifra = await new ClassicBirokratSifrantPersistor(client, null, new BirokratArtikelRetrieverSearchStrategy(client)).SearchThenIfNotFoundCreate(args);
                postavka.BirokratSifra = sifra;
                
                postavke.Add(postavka);
            }
            return postavke;
        }
    }

    public interface IOrderAddOpCondition {
        public bool Is(WoocommerceOrder order);
    }

    public class PaymentMethod_OrderAddOpCondition : IOrderAddOpCondition
    {

        private string paymentMethodValue;
        public PaymentMethod_OrderAddOpCondition(string paymentMethodValue) {
            this.paymentMethodValue = paymentMethodValue;
        }
        public bool Is(WoocommerceOrder order)
        {
            return order.Data.PaymentMethod == paymentMethodValue;
        }
    }

    public class Currency_OrderAddOpCondition : IOrderAddOpCondition
    {
        string currency;
        public Currency_OrderAddOpCondition(string currency) {
            this.currency = currency;
        }
        public bool Is(WoocommerceOrder order)
        {
            return order.Data.Currency == "";
        }
    }

    public class AndX : IOrderAddOpCondition
    {
        IOrderAddOpCondition op1;
        IOrderAddOpCondition op2;
        public AndX(IOrderAddOpCondition op1, IOrderAddOpCondition op2) {
            this.op1 = op1;
            this.op2 = op2;
        }
        public bool Is(WoocommerceOrder order)
        {
            return op1.Is(order) && op2.Is(order);
        }
    }

    public class OrX : IOrderAddOpCondition
    {
        IOrderAddOpCondition op1;
        IOrderAddOpCondition op2;
        public OrX(IOrderAddOpCondition op1, IOrderAddOpCondition op2)
        {
            this.op1 = op1;
            this.op2 = op2;
        }
        public bool Is(WoocommerceOrder order)
        {
            return op1.Is(order) || op2.Is(order);
        }
    }

    public class NotX : IOrderAddOpCondition
    {
        IOrderAddOpCondition op;
        public NotX(IOrderAddOpCondition op) {
            this.op = op;
        }
        public bool Is(WoocommerceOrder order)
        {
            return !op.Is(order);
        }
    }
    public class ShippingCountryIsEuX : IOrderAddOpCondition
    {
        public bool Is(WoocommerceOrder order)
        {
            return Tools.IsEUWooCountry(order.Data.Shipping.Country);
        }
    }

    public class VatExemptX : IOrderAddOpCondition
    {
        public bool Is(WoocommerceOrder order)
        {
            return GWooOps.IsVatExempt(order);
        }
    }
}
