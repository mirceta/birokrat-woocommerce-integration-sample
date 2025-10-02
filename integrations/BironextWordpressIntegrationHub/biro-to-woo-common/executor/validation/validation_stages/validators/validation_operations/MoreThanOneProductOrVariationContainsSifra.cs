using birowoo_exceptions;
using core.structs;
using System;
using System.Linq;

namespace biro_to_woo_common.executor.validation_stages.validators.validation_operations
{
    public class MoreThanOneProductOrVariationContainsSku : IProductTransferVerifyOperation
    {

        string skuField;

        public MoreThanOneProductOrVariationContainsSku(
            string skuField)
        {
            this.skuField = skuField;
        }

        public void Verify(string sku, BiroOutComparisonContext context)
        {

            ProductTransferVerifyOperationHelper.nullGuards(sku, context);

            // is there any element in context.biroItems where element["sku"] == sifra
            var matches = context.outItems.Where(element => element["sku"] as string == sku).ToList();

            if (matches.Count == 0)
            {
                throw new CannotValidateNonSyncedProductException("No products or variations found with the specified sku.");
            }
            else if (matches.Count == 1)
            {
                return;
            }

            var all = matches.Select(match => $"Parent: {match["parent_id"]} Id: {match["id"]} Sku: {match["sku"]}").ToList();
            throw new IntegrationProcessingException($"Sku se lahko pojavi samo v enem izdelku, vendar imajo naslednji artikli enak sku: {string.Join(",", all)}");
        }
    }
}
