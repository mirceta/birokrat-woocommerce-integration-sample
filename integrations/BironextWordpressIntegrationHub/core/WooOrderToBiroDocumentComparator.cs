using BironextWordpressIntegrationHub.structs;
using birowoo_exceptions;
using BiroWoocommerceHub.flows;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using tests.tests.estrada;

namespace tests.tools
{

    public enum ValidationErrorType
    {
        None,
        Davcna,
        BuyerName,
        Country,
        City,
        Postcode,
        Address,
        ShippingName,
        ShippingCountry,
        ShippingCity,
        ShippingPostcode,
        ShippingAddress,
        TotalPrice,
        TaxAmount
    }

    public class ComparisonResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationErrorType ErrorType { get; set; }

        public static ComparisonResult SuccessResult()
        {
            return new ComparisonResult { Success = true, ErrorType = ValidationErrorType.None };
        }

        public static ComparisonResult FailureResult(string errorMessage, ValidationErrorType errorType)
        {
            return new ComparisonResult { Success = false, ErrorMessage = errorMessage, ErrorType = errorType };
        }
    }


    public class SkipCompare {
        public bool Country { get; set; }
    }
    public class WooOrderToBiroDocumentComparator {

        bool debug;
        SkipCompare skip;
        public WooOrderToBiroDocumentComparator(bool debug = false, SkipCompare skip = null) {
            this.debug = debug;
            if (skip == null) {
                this.skip = new SkipCompare() {
                    Country = false,
                };
            } else 
                this.skip = skip;
        }


        public async Task<ComparisonResult> Compare(WoocommerceOrder order, string xml, ValidationComponents components)
        {
            if (string.IsNullOrEmpty(xml))
                return ComparisonResult.FailureResult("XML is empty", ValidationErrorType.None);

            var biroCmp = new EslogParser().ParseComparisonAttributes(order, xml);
            var wooCmp = new WooJsonParser().ParseComparisonAttributes(order, components);

            if (!string.IsNullOrEmpty(wooCmp.Davcna) && wooCmp.Davcna != biroCmp.Davcna)
            {
                if (debug)
                {
                    Debugger.Break();
                }
                else
                {
                    var msg = ValidationErrorMsg("buyer taxnum", wooCmp.Davcna, biroCmp.Davcna, ComparatorsToString(wooCmp, biroCmp));
                    msg += "Ali imate v šifrantu držav šifre INTRASTAT nastavljene pravilno?";
                    return ComparisonResult.FailureResult(msg, ValidationErrorType.Davcna);
                }
            }

            if (!complexStringComparison(wooCmp.Nazivbuyer, biroCmp.Nazivbuyer))
            {
                if (debug)
                {
                    Debugger.Break();
                }
                else
                {
                    var msg = ValidationErrorMsg("buyer name", wooCmp.Nazivbuyer, biroCmp.Nazivbuyer, ComparatorsToString(wooCmp, biroCmp));
                    return ComparisonResult.FailureResult(msg, ValidationErrorType.BuyerName);
                }
            }

            string mapperctry = wooCmp.Drzavabuyer;
            if (components.CountryMapper != null)
            {
                mapperctry = await components.CountryMapper.Map(wooCmp.Drzavabuyer);
            }

            if (!skip.Country && wooCmp.Drzavabuyer != biroCmp.Drzavabuyer && mapperctry != biroCmp.Drzavabuyer)
            {
                if (debug)
                {
                    Debugger.Break();
                }
                else
                {
                    var msg = ValidationErrorMsg("buyer country", wooCmp.Drzavabuyer, biroCmp.Drzavabuyer, ComparatorsToString(wooCmp, biroCmp));
                    return ComparisonResult.FailureResult(msg, ValidationErrorType.Country);
                }
            }

            if (wooCmp.Postabuyer != biroCmp.Postabuyer)
            {
                if (debug)
                {
                    Debugger.Break();
                }
                else
                {
                    var msg = ValidationErrorMsg("buyer city", wooCmp.Postabuyer, biroCmp.Postabuyer, ComparatorsToString(wooCmp, biroCmp));
                    return ComparisonResult.FailureResult(msg, ValidationErrorType.City);
                }
            }

            if (wooCmp.Postnastevilkabuyer != biroCmp.Postnastevilkabuyer)
            {
                if (debug)
                {
                    Debugger.Break();
                }
                else
                {
                    var msg = ValidationErrorMsg("buyer postcode", wooCmp.Postnastevilkabuyer, biroCmp.Postnastevilkabuyer, ComparatorsToString(wooCmp, biroCmp));
                    return ComparisonResult.FailureResult(msg, ValidationErrorType.Postcode);
                }
            }

            if (!complexStringComparison(wooCmp.Naslovbuyer, biroCmp.Naslovbuyer))
            {
                if (debug)
                {
                    Debugger.Break();
                }
                else
                {
                    var msg = ValidationErrorMsg("buyer address", wooCmp.Naslovbuyer, biroCmp.Naslovbuyer, ComparatorsToString(wooCmp, biroCmp));
                    return ComparisonResult.FailureResult(msg, ValidationErrorType.Address);
                }
            }

            // Check shipper
            if (!string.IsNullOrEmpty(biroCmp.Nazivshipping) || !string.IsNullOrEmpty(biroCmp.Drzavashipping) ||
                !string.IsNullOrEmpty(biroCmp.Postashipping) || !string.IsNullOrEmpty(biroCmp.Naslovshipping) ||
                wooCmp.Nazivshipping != wooCmp.Nazivbuyer || wooCmp.Drzavashipping != wooCmp.Drzavabuyer ||
                wooCmp.Postashipping != wooCmp.Postabuyer || wooCmp.Naslovshipping != wooCmp.Naslovbuyer)
            {
                if (!complexStringComparison(wooCmp.Nazivshipping, biroCmp.Nazivshipping) && wooCmp.Nazivshipping != wooCmp.Nazivbuyer)
                {
                    if (debug)
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        var msg = ValidationErrorMsg("shipping name", wooCmp.Nazivshipping, biroCmp.Nazivshipping, ComparatorsToString(wooCmp, biroCmp));
                        return ComparisonResult.FailureResult(msg, ValidationErrorType.ShippingName);
                    }
                }

                mapperctry = wooCmp.Drzavashipping;
                if (components.CountryMapper != null)
                {
                    mapperctry = await components.CountryMapper.Map(wooCmp.Drzavashipping);
                }

                if (!skip.Country && wooCmp.Drzavashipping != biroCmp.Drzavashipping && mapperctry != biroCmp.Drzavashipping)
                {
                    if (debug)
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        var msg = ValidationErrorMsg("shipping country", wooCmp.Drzavashipping, biroCmp.Drzavashipping, ComparatorsToString(wooCmp, biroCmp));
                        return ComparisonResult.FailureResult(msg, ValidationErrorType.ShippingCountry);
                    }
                }

                if (wooCmp.Postashipping != biroCmp.Postashipping)
                {
                    if (debug)
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        var msg = ValidationErrorMsg("shipping city", wooCmp.Postashipping, biroCmp.Postashipping, ComparatorsToString(wooCmp, biroCmp));
                        return ComparisonResult.FailureResult(msg, ValidationErrorType.ShippingCity);
                    }
                }

                if (wooCmp.Postnastevilkashipping != biroCmp.Postnastevilkashipping)
                {
                    if (debug)
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        var msg = ValidationErrorMsg("shipping postcode", wooCmp.Postnastevilkashipping, biroCmp.Postnastevilkashipping, ComparatorsToString(wooCmp, biroCmp));
                        return ComparisonResult.FailureResult(msg, ValidationErrorType.ShippingPostcode);
                    }
                }

                if (!complexStringComparison(wooCmp.Naslovshipping, biroCmp.Naslovshipping))
                {
                    if (debug)
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        var msg = ValidationErrorMsg("shipping address", wooCmp.Naslovshipping, biroCmp.Naslovshipping, ComparatorsToString(wooCmp, biroCmp));
                        return ComparisonResult.FailureResult(msg, ValidationErrorType.ShippingAddress);
                    }
                }
            }

            // Check other attributes
            if (Math.Abs(wooCmp.SteviloPostavk - biroCmp.SteviloPostavk) > 2)
            {
                if (debug) Debugger.Break();
            }

            var diff = Math.Abs(Tools.ParseDoubleBigBrainTime(wooCmp.Total) - Tools.ParseDoubleBigBrainTime(biroCmp.Total));
            if (diff >= 0.005)
            {
                if (debug) Debugger.Break();
                else
                {
                    var priceErrorResult = handlePriceError(false, order, biroCmp, wooCmp, diff);
                    if (!priceErrorResult.Success)
                        return priceErrorResult;
                }
            }

            diff = Math.Abs(Tools.ParseDoubleBigBrainTime(wooCmp.Totaltax) - Tools.ParseDoubleBigBrainTime(biroCmp.Totaltax));
            if (diff >= 0.005)
            {
                if (debug) Debugger.Break();
                else
                {
                    var taxErrorResult = handlePriceError(true, order, biroCmp, wooCmp, diff);
                    if (!taxErrorResult.Success)
                        return taxErrorResult;
                }
            }

            return ComparisonResult.SuccessResult();
        }

        private ComparisonResult handlePriceError(bool is_tax_error, WoocommerceOrder order, ComparisonAttributes biroCmp, ComparisonAttributes wooCmp, double diff)
        {
            string result = new VerifyWhetherWoocommerceRoundingIsOk().AreOrderValuesConsistent(order);
            if (Math.Round(diff * 100) < 2)
            {
                if (is_tax_error)
                    return ComparisonResult.FailureResult(
                        $"1 cent difference in total tax: {wooCmp.Totaltax} birokrat: {biroCmp.Totaltax}" + ComparatorsToString(wooCmp, biroCmp), ValidationErrorType.TaxAmount);
                else
                    return ComparisonResult.FailureResult(
                        $"1 cent difference in total price. out: {wooCmp.Total} birokrat: {biroCmp.Total}" + ComparatorsToString(wooCmp, biroCmp), ValidationErrorType.TotalPrice);
            }
            else
            {
                return ComparisonResult.FailureResult(
                    ValidationErrorMsg("total price or tax amount", wooCmp.Totaltax, biroCmp.Totaltax, ComparatorsToString(wooCmp, biroCmp)), ValidationErrorType.TotalPrice);
            }
        }

        bool complexStringComparison(string tmp1, string tmp2) {
            if (tmp1 != null) {
                tmp1 = trimgood(tmp1).ToLower();
            }
            if (tmp2 != null) {
                tmp2 = trimgood(tmp2).ToLower();
            }
            return tmp1 == tmp2;
        }

        string trimgood(string tmp) {
            return tmp.Replace("\n", "").Replace(" ", "").Replace("\r", "").Replace("\t", "");
        }

        string ValidationErrorMsg(string attr, string wooval, string biroval, string full) {
            return $"The {attr} was inconsistent between the order and the birokrat document! order: {wooval}, birokrat document: {biroval}" + full;
        }

        string ComparatorsToString(ComparisonAttributes wooCmp, ComparisonAttributes biroCmp) {
            string some = "";
            some += $"Woo: {wooCmp.Davcna} Biro: {biroCmp.Davcna}" + "\n";

            some += $"Woo: {wooCmp.Nazivbuyer} Biro: {biroCmp.Nazivbuyer}" + "\n";
            some += $"Woo: {wooCmp.Drzavabuyer} Biro: {biroCmp.Drzavabuyer}" + "\n";
            some += $"Woo: {wooCmp.Postabuyer} Biro: {biroCmp.Postabuyer}" + "\n";
            some += $"Woo: {wooCmp.Postnastevilkabuyer} Biro: {biroCmp.Postnastevilkabuyer}" + "\n";
            some += $"Woo: {wooCmp.Naslovbuyer} Biro: {biroCmp.Naslovbuyer}" + "\n";


            some += $"Woo: {wooCmp.Nazivshipping} Biro: {biroCmp.Nazivshipping}" + "\n";
            some += $"Woo: {wooCmp.Drzavashipping} Biro: {biroCmp.Drzavashipping}" + "\n";
            some += $"Woo: {wooCmp.Postashipping} Biro: {biroCmp.Postashipping}" + "\n";
            some += $"Woo: {wooCmp.Postnastevilkashipping} Biro: {biroCmp.Postnastevilkashipping}" + "\n";
            some += $"Woo: {wooCmp.Naslovshipping} Biro: {biroCmp.Naslovshipping}" + "\n";

            some += $"Woo: {wooCmp.SteviloPostavk} Biro: {biroCmp.SteviloPostavk}" + "\n";
            some += $"Woo: {wooCmp.Total} Biro: {biroCmp.Total}" + "\n";
            some += $"Woo: {wooCmp.Totaltax} Biro: {biroCmp.Totaltax}" + "\n";
            return some;
        }




    }

    public class WooJsonParser {

        public WooJsonParser() {
        }

        public ComparisonAttributes ParseComparisonAttributes(WoocommerceOrder order, ValidationComponents components) {
            
            ComparisonAttributes attrs = new ComparisonAttributes();

            // buyer
            attrs.Davcna = components.VatIdParser.Get(order).GetAwaiter().GetResult();
            attrs.Nazivbuyer = order.Data.Billing.FirstName + " " + order.Data.Billing.LastName;
            attrs.Drzavabuyer = order.Data.Billing.Country;
            attrs.Postabuyer = order.Data.Billing.City;
            attrs.Postnastevilkabuyer = order.Data.Billing.Postcode;
            attrs.Naslovbuyer = order.Data.Billing.Address1 + order.Data.Billing.Address2;

            // shipper
            attrs.Nazivshipping = order.Data.Shipping.FirstName + " " + order.Data.Shipping.LastName;
            attrs.Drzavashipping = order.Data.Shipping.Country;
            attrs.Postashipping = order.Data.Shipping.City;
            attrs.Postnastevilkashipping = order.Data.Shipping.Postcode;
            attrs.Naslovshipping = order.Data.Shipping.Address1 + order.Data.Shipping.Address2;


            attrs.SteviloPostavk = order.Items.Count;
            //attrs.SifrePostavk = extractor.ExtractFromOrder(order).GetAwaiter().GetResult().Select(x => x.BirokratSifra).ToList();
            attrs.Total = order.Data.Total;
            attrs.Totaltax = order.Data.TotalTax;

            return attrs;
        }
    }
    class Partner {
        public string vrsta;
        public string naziv;
        public string kodadrzave;
        public string ulica;
        public string postnastevilka;
        public string mesto;
        public string davcna;
    }

    public class ComparisonAttributes {
        string total;
        string totaltax;

        string nazivbuyer;
        string davcna;
        string drzavabuyer;
        string naslovbuyer;
        string postnastevilkabuyer;
        string postabuyer;

        string nazivshipping;
        string drzavashipping;
        string naslovshipping;
        string postnastevilkashipping;
        string postashipping;
        
        int steviloPostavk;
        List<string> sifrePostavk;

        public string Total { get => total; set => total = value; }
        public string Totaltax { get => totaltax; set => totaltax = value; }
        public string Nazivbuyer { get => nazivbuyer; set => nazivbuyer = value; }
        public string Davcna { get => davcna; set => davcna = value; }
        public string Drzavabuyer { get => drzavabuyer; set => drzavabuyer = value; }
        public string Naslovbuyer { get => naslovbuyer; set => naslovbuyer = value; }
        public string Postnastevilkabuyer { get => postnastevilkabuyer; set => postnastevilkabuyer = value; }
        public string Postabuyer { get => postabuyer; set => postabuyer = value; }
        public string Nazivshipping { get => nazivshipping; set => nazivshipping = value; }
        public string Drzavashipping { get => drzavashipping; set => drzavashipping = value; }
        public string Naslovshipping { get => naslovshipping; set => naslovshipping = value; }
        public string Postnastevilkashipping { get => postnastevilkashipping; set => postnastevilkashipping = value; }
        public string Postashipping { get => postashipping; set => postashipping = value; }
        public int SteviloPostavk { get => steviloPostavk; set => steviloPostavk = value; }
        public List<string> SifrePostavk { get => sifrePostavk; set => sifrePostavk = value; }
    }
}
