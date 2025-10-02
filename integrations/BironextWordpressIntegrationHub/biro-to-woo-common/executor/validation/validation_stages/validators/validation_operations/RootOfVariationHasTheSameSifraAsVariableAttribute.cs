using biro_to_woo_common.executor.validation_stages.validators;
using biro_to_woo_common.executor.validation_stages.validators.validation_operations;
using birowoo_exceptions;
using core.structs;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace biro_to_woo_common.executor.validation.validation_stages.validators.validation_operations
{
    public class RootOfVariationHasTheSameSifraAsVariableAttribute : IProductTransferVerifyOperation
    {
        string skuField;
        string variationField;
        public RootOfVariationHasTheSameSifraAsVariableAttribute(string skuField, string variationField) { 
            this.skuField = skuField;
            this.variationField = variationField;
        }

        public void Verify(string sifra, BiroOutComparisonContext context)
        {
            ProductTransferVerifyOperationHelper.nullGuards(sifra, context);
            if (context.biroItems.Count == 0 || context.outItems.Count == 0)
                throw new IntegrationProcessingException("Ni podatkov");

            var biroItem = context.biroItems.FirstOrDefault(x => x[skuField] as string == sifra);

            var matches = context.outItems.Where(element => element["sku"] as string == sifra).ToList();

            foreach (var x in matches) // are we sure there can be multiple matches??
            {
                string parentid = GWooOps.SerializeIntWooProperty(x["parent_id"]);
                if (string.IsNullOrEmpty(parentid) || parentid == "0") {
                    // has no parent
                    continue;
                }
                // has parent

                var parent = context.outItems.Where(x => GWooOps.SerializeIntWooProperty(x["id"]) == parentid);
                if (parent.Count() == 0)
                    throw new IntegrationProcessingException($"Korenskega izdelka od izdelka s šifro {sifra} nismo našli na spletni trgovini");
                if (parent.Count() > 1)
                    throw new IntegrationProcessingException($"V množici obstaja več izdelkov s šifro {sifra}");

                var prnt = parent.Single();
                if ((string)prnt["sku"] != (string)biroItem[variationField])
                    throw new IntegrationProcessingException($"Korenski izdelek izdelka {sifra} mora imeti šifro enako polju {variationField} v variacijah, ki spadajo pod ta korenski izdelek.");

            }
        }
    }
}
