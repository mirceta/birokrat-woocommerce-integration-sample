using birowoo_exceptions;
using Castle.Core.Internal;
using core.structs;
using core.tools.wooops;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace biro_to_woo_common.executor.validation_stages.validators.validation_operations
{

    public class ProductOutTypeMismatch : IProductTransferVerifyOperation
    {
        string variableProductBirokratField;
        string skuField;

        public ProductOutTypeMismatch(string variableProductBirokratField, string skuField)
        {
            this.variableProductBirokratField = variableProductBirokratField;
            this.skuField = skuField;
        }

        public void Verify(string sifra, BiroOutComparisonContext comparisonContext)
        {

            // what if there is no match?
            var biroMatch = comparisonContext.biroItems.Single(x => x[skuField] as string == sifra);

            // we allow multiple matches for multilingo - sometimes multiple woo products can map to one birokrat product, because one is englight, slovenian, cro version.
            var wooMatches = comparisonContext.outItems.Where(x => x["sku"] as string == sifra);

            foreach (var wooMatch in wooMatches)
            {
                if (!string.IsNullOrEmpty((string)biroMatch[variableProductBirokratField]))
                {
                    if (biroArtikelIsVariable(biroMatch))
                    {
                        wooMatchShouldBeVariableProduct(sifra, wooMatch, biroMatch);
                    }
                }

                if (!biroArtikelIsVariable(biroMatch))
                {
                    wooMatchShouldBeSimpleProduct(sifra, wooMatch, biroMatch);
                }
            }
        }


        private bool biroArtikelIsVariable(Dictionary<string, object> biroMatch)
        {
            return biroMatch.ContainsKey(variableProductBirokratField) && !string.IsNullOrEmpty((string)biroMatch[variableProductBirokratField]);
        }

        private void wooMatchShouldBeVariableProduct(string sifra, Dictionary<string, object> wooMatch, Dictionary<string, object> biroMatch)
        {
            if (wooMatch.ContainsKey("parent_id") && 
                wooMatch["parent_id"] != null && 
                !GWooOps.SerializeIntWooProperty(wooMatch["parent_id"]).Equals("0"))
            {

            }
            else
            {
                string msg = $"Produkt s šifro {sifra} je na spletni trgovini enostaven in v birokratu variabilen.";
                msg += $" V birokratu je {variableProductBirokratField}={(string)biroMatch[variableProductBirokratField]}.";
                msg += $" Na spletni trgovini je ta produkt enostaven.";
                msg += $" Prosimo poskrbite, da bo produkt enakega tipa na obeh straneh.";
                throw new IntegrationProcessingException(msg);
            }
        }

        private void wooMatchShouldBeSimpleProduct(string sifra, Dictionary<string, object> wooMatch, Dictionary<string, object> biroMatch)
        {
            if (wooMatch.ContainsKey("parent_id") && 
                wooMatch["parent_id"] != null && 
                !GWooOps.SerializeIntWooProperty(wooMatch["parent_id"]).Equals("0"))
            {
                string msg = $"Produkt s šifro {sifra} je na spletni trgovini variabilen in v birokratu enostaven.";
                msg += $" V birokratu je atribut {variableProductBirokratField} prazno.";
                msg += $" Na spletni trgovini je ta produkt variabilen (je variacija produkta z ID={GWooOps.SerializeIntWooProperty(wooMatch["parent_id"])}).";
                msg += $" Prosimo poskrbite, da bo produkt enakega tipa na obeh straneh.";
                throw new IntegrationProcessingException(msg);
            }
        }
    }
}
