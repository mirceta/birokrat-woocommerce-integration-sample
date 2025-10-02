using birowoo_exceptions;
using core.structs;
using System.Collections.Generic;
using System.Linq;

namespace biro_to_woo_common.executor.validation_stages.validators.validation_operations
{
    public class Verify_That_Sifra_NotSameAsExistingValueForVariableAttribute : IProductTransferVerifyOperation
    {

        string skuField;
        string variableField;

        /// <summary>
        /// This condition must be met because we want to enforce that each product on the spletna trgovina has an uniquie SKU code.
        /// If the variation aqttribute and the sifra are the same, then this will create 2 products - the root product that will have the same sku code
        /// and the variation that will have the same sku code. This means that when we retrieve the whole database from the web shop we will have
        /// the same sku code for different products. This is a violation even though the products are related (parent and child)
        /// </summary>
        public Verify_That_Sifra_NotSameAsExistingValueForVariableAttribute(string skuField, string variableField)
        {
            this.skuField = skuField;
            this.variableField = variableField;
        }

        public void Verify(string sifra, BiroOutComparisonContext context)
        {

            ProductTransferVerifyOperationHelper.nullGuards(sifra, context);

            var biroItem = context.biroItems.FirstOrDefault(x => x[skuField] as string == sifra);

            var othrItem = context.biroItems.FirstOrDefault(x => x[variableField] as string == sifra);

            if (biroItem == null)
            {
                throw new KeyNotFoundException($"Could not find the item with SKU {sifra}.");
            }


            if (biroItem != null && othrItem != null)
            {
                string tmp = $"Šifra se pojavi v vrednosti variacijskega atributa ({variableField}) v Birokratu pri izdelku:";
                tmp += $" {sifra}: {othrItem[skuField]}.";
                throw new IntegrationProcessingException(tmp);
            }


        }
    }
}
