using BiroWoocommerceHub.flows;
using core.logic.common_birokrat;
using core.tools.wooops;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;

namespace biro_to_woo_common.executor.validation_stages.change_trackers.exhaustive.common
{

    public interface IProductComparer {
        bool AddOnPriceChange(HashSet<string> sifrasDiff, Dictionary<string, object> artikel, Dictionary<string, object> product);

        bool AddOnZalogaChange(HashSet<string> sifrasDiff, Dictionary<string, object> artikel, Dictionary<string, object> product);
    }

    class ProductComparer : IProductComparer
    {

        bool verbose;
        IMyLogger logger;
        public ProductComparer(bool verbose, IMyLogger logger)
        {
            this.verbose = verbose;
            this.logger = logger;
        }
        public bool AddOnPriceChange(HashSet<string> sifrasDiff, Dictionary<string, object> artikel, Dictionary<string, object> product)
        {

            string sifraFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla);
            string priceFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.PCsPD);

            string wooprice = GWooOps.SerializeDblWooProperty(product["regular_price"]);
            if (GWooOps.SerializeDblWooProperty(product["regular_price"]) == "0")
            {
                wooprice = GWooOps.SerializeDblWooProperty(product["price"]);
            }

            double biroprice = Tools.ParseDoubleBigBrainTime((string)artikel[priceFieldName]);
            double woopric = Tools.ParseDoubleBigBrainTime(wooprice);

            if (Math.Abs(biroprice - woopric) > 0.02)
            {
                ConsolePrintout($"sifra: {(string)artikel[sifraFieldName]} sku: {(string)product["sku"]} cena biro: {(string)artikel[priceFieldName]} woo: {wooprice}");
                return true;
            }
            return false;
        }

        public bool AddOnZalogaChange(HashSet<string> sifrasDiff, Dictionary<string, object> artikel, Dictionary<string, object> product)
        {

            string sifraFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla);

            if (Tools.ParseDoubleBigBrainTime((string)artikel["zaloga"]) != Tools.ParseDoubleBigBrainTime(GWooOps.SerializeIntWooProperty(product["stock_quantity"])))
            {
                ConsolePrintout($"sifra: {(string)artikel[sifraFieldName]} woosku: {(string)product["sku"]} zaloga biro: {(string)artikel["zaloga"]} woo: {GWooOps.SerializeIntWooProperty(product["stock_quantity"])}");
                return true;
            }
            return false;
        }

        private void ConsolePrintout(string content)
        {
            if (verbose)
            {
                Console.WriteLine(content);
                if (logger != null)
                {
                    logger.LogInformation(content);
                }
            }

        }
    }
}
