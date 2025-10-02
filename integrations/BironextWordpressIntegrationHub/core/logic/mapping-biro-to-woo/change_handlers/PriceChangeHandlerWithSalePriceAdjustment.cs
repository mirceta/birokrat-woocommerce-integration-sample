using birowoo_exceptions;
using BiroWoocommerceHub.flows;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Text;

namespace core.logic.mapping_biro_to_woo.change_handlers
{
    public class PriceChangeHandlerWithSalePriceAdjustment : IBirokratProductChangeHandler
    {

        string woo_decimal_separator;
        public PriceChangeHandlerWithSalePriceAdjustment(string woo_decimal_separator = ",") {
            this.woo_decimal_separator = woo_decimal_separator;
        }

        public void HandleChange(Dictionary<string, object> biroArtikel, Dictionary<string, object> obj, Dictionary<string, object> wooload)
        {
            // handle price changes
            string biropricewithtax = ((string)biroArtikel["PCsPD"]);
            string strregpricewtax = GWooOps.SerializeDblWooProperty(obj["regular_price"]);
            if (GWooOps.SerializeDblWooProperty(obj["regular_price"]) == "0") {
                strregpricewtax = GWooOps.SerializeDblWooProperty(obj["price"]);
            }
            string strsalepricewtax = GWooOps.SerializeDblWooProperty(obj["sale_price"]);

            Console.WriteLine($"PC detector: biro={biropricewithtax} outregprice={strregpricewtax}");


            if (Tools.ParseDoubleBigBrainTime(biropricewithtax) != 0 &&
                Tools.ParseDoubleBigBrainTime(biropricewithtax) != Tools.ParseDoubleBigBrainTime(strregpricewtax))
            {

                Console.WriteLine($"Price detector: change detected");

                if (GWooOps.SerializeDblWooProperty(obj["regular_price"]) == "0") {
                    wooload["price"] = fix_price(biropricewithtax);
                    wooload["regular_price"] = fix_price(biropricewithtax);
                } else {
                    wooload["regular_price"] = fix_price(biropricewithtax);
                }


                if (!string.IsNullOrEmpty(strsalepricewtax) && strregpricewtax != strsalepricewtax)
                {
                    // correct sale price
                    /*
                    double biroprice = Tools.ParseDoubleBigBrainTime(biropricewithtax);
                    double regprice = Tools.ParseDoubleBigBrainTime(strregpricewtax);
                    double saleprice = Tools.ParseDoubleBigBrainTime(strsalepricewtax);

                    double birosaleprice = biroprice * (saleprice / regprice);
                    birosaleprice = Math.Ceiling(birosaleprice) - 0.10;
                    wooload["sale_price"] = biropricewithtax.Replace(".", ",");
                    */
                }
            }
        }

        private string fix_price(string price) {

            var tmp = Tools.ParseDoubleBigBrainTime(price);
            price = tmp.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            if (woo_decimal_separator == ",") {
                return price.Replace(".", ",");
            } else if (woo_decimal_separator == ".") {
                return price.Replace(",", ".");
            } else {
                throw new DecimalSeparatorException("The defined decimal separator is not acceptable");
            }
        }
    }

    public class DecimalSeparatorException : IntegrationProcessingException {
        public DecimalSeparatorException(string message) : base(message) { 
        
        }
    }
}