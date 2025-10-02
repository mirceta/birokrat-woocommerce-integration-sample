using BironextWordpressIntegrationHub.structs;
using Newtonsoft.Json;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace webshop_client_shopify
{
    public class ShopifyToWooOrderAdapter
    {

        public ShopifyToWooOrderAdapter() { }

        public WoocommerceOrder Adapt(Order order) {
            var woo = new WoocommerceOrder();
            var some = JsonConvert.SerializeObject(woo);
            woo = JsonConvert.DeserializeObject<WoocommerceOrder>(some);

            Data data = SetBasics(order);
            SetBillingPerson(order, data);
            SetShippingPerson(order, data);
            ShippingExpenses(order, woo, data);

            woo.Data = data;

            woo.Items = AdaptOrderItems(order);

            return woo;
        }

        private static List<WoocommerceOrderItem> AdaptOrderItems(Order order) {
            var items = order.LineItems.ToList();

            List<WoocommerceOrderItem> wooitems = new List<WoocommerceOrderItem>();
            foreach (var item in items) {
                WoocommerceOrderItem wooitem = new WoocommerceOrderItem();


                if (item.TaxLines.ToList().Count != 1)
                    throw new Exception($"Could not handle item {item.SKU} with multiple or 0 tax lines. Needs to be exactly 1!");

                var tl = item.TaxLines.ToList()[0];

                wooitem.Subtotal = DecimalToString(item.Price - tl.Price);
                wooitem.SubtotalTax = DecimalToString(tl.Price);
                wooitem.Total = DecimalToString(item.Price - tl.Price);
                wooitem.TotalTax = DecimalToString(tl.Price);
                wooitem.Quantity = (int)item.Quantity;

                var originProduct = new Dictionary<string, object>();
                originProduct["sku"] = item.SKU;

                wooitem.OriginProduct = originProduct;

                wooitems.Add(wooitem);
            }

            return wooitems;
        }

        private static void ShippingExpenses(Order order, WoocommerceOrder woo, Data data) {
            if (order.ShippingLines.Count() > 1) {
                throw new Exception("Don't know how to handle multiple shippings");
            }
            if (order.ShippingLines.Count() == 1) {
                var tmp = order.ShippingLines.ToList()[0];

                if (tmp.TaxLines.Count() > 1)
                    throw new Exception("Don't know how to handle multiple tax lines in shipping");
                var tmp2 = tmp.TaxLines.ToList()[0];
                data.ShippingTax = DecimalToString(tmp2.Price);

                data.ShippingTotal = DecimalToString(tmp.Price - tmp2.Price);

                woo.ShippingMethod = tmp.Code;
            }
        }

        private static Data SetBasics(Order order) {
            var data = new Data();
            data.Id = (int)order.OrderNumber;
            data.Number = ((int)order.OrderNumber).ToString();

            data.Status = order.FinancialStatus == "paid" ? "completed" : "processing";

            data.Currency = order.Currency;

            data.PricesIncludeTax = (bool)order.TaxesIncluded;


            DateTime crdate = order.CreatedAt.Value.DateTime;
            data.DateCreated = new DateCreated() {
                Date = crdate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + ".000000",
                Timezone = "",
                TimezoneType = 0
            };

            data.Total = DecimalToString(order.TotalPrice);
            data.TotalTax = DecimalToString(order.TotalTax);


            if (order.PaymentGatewayNames.Count() != 1) {
                throw new Exception("Cannot handle orders with none or multiple payment gateways");
            }
            data.PaymentMethod = order.PaymentGatewayNames.ToList()[0];

            data.MetaData = new List<MetaData>();
            data.MetaData.Add(new MetaData() {
                Id = 0,
                Key = "_billing_vat",
                Value = "" // ????????
            });
            data.MetaData.Add(new MetaData() {
                Id = 0,
                Key = "is_vat_exempt",
                Value = "no" // ????????
            });
            return data;
        }

        private static void SetShippingPerson(Order order, Data data) {
            data.Shipping = new BironextWordpressIntegrationHub.structs.Shipping();
            data.Shipping.FirstName = order.ShippingAddress.FirstName;
            data.Shipping.LastName = order.ShippingAddress.LastName;
            data.Shipping.Company = order.ShippingAddress.Company;
            data.Shipping.Country = order.ShippingAddress.CountryCode;
            data.Shipping.City = order.ShippingAddress.City;
            data.Shipping.Address1 = order.ShippingAddress.Address1;
            data.Shipping.Address2 = order.ShippingAddress.Address2;
            data.Shipping.Postcode = order.ShippingAddress.Zip;
        }

        private static void SetBillingPerson(Order order, Data data) {
            data.Billing = new Billing();
            data.Billing.FirstName = order.BillingAddress.FirstName;
            data.Billing.LastName = order.BillingAddress.LastName;
            data.Billing.Country = order.BillingAddress.CountryCode; // seems that country codes are the same in woo and shopify
            data.Billing.City = order.BillingAddress.City;
            data.Billing.Postcode = order.BillingAddress.Zip;
            data.Billing.Address1 = order.BillingAddress.Address1;
            data.Billing.Address2 = order.BillingAddress.Address2;
            data.Billing.Company = order.BillingAddress.Company;
            data.Billing.Email = order.Customer.Email;
            data.Billing.Email = order.Email; // which one is correct?
            data.Billing.Phone = order.Customer.Phone;
        }

        private static string DecimalToString(decimal? dec) {
            return ((double)dec).ToString("0.00");
        }
    }
}
