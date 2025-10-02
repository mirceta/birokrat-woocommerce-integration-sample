using birowoo_exceptions;
using core.structs;
using System;
using System.Linq;

namespace biro_to_woo_common.executor.validation_stages.validators.validation_operations
{
    public class AllowSKUToBeInMultipleProducts_If_NoneOfTheseProductsIsADraft : IProductTransferVerifyOperation
    {

        string skuField;


        /// <summary>
        /// This is about internationalization. A product can be in many languages - can have SLO, HR, ENG etc, in special cases
        /// where this is a customer requirement. But because of an unknown reason, having 2 products with the same SKU code and one
        /// being a draft and one not is an illegal state. It is an illegal state because TODO
        /// </summary>
        public AllowSKUToBeInMultipleProducts_If_NoneOfTheseProductsIsADraft(
            string skuField)
        {
            this.skuField = skuField;
        }

        public void Verify(string sku, BiroOutComparisonContext context)
        {
            // is there any element in context.biroItems where element["sku"] == sifra

            if (sku == null)
                throw new ArgumentNullException(nameof(sku));
            if (context == null)
                throw new ArgumentNullException("context");

            var matches = context.outItems.Where(element => element["sku"] as string == sku).ToList();

            if (matches.Count == 0)
            {
                throw new CannotValidateNonSyncedProductException("No products or variations found with the specified sku.");
            }
            else if (matches.Count == 1)
            {
                return;
            }

            var distinctStatuses = matches.Select(x => (string)x["status"]).Distinct().ToList();

            if (distinctStatuses.Count > 1)
            {
                var all = matches.Select(match => $"Parent: {match["parent_id"]} Id: {match["id"]} Sku: {match["sku"]} Status: {match["status"]}").ToList();
                throw new IntegrationProcessingException($"Produkti imajo na spletni trgovini isto SKU kodo, vendar različen status produkta! Tega ne dovoljujemo!! Detajli: {string.Join(",", all)}");
            }

            // This fits in this class well - we are normally not permitting multiple products with the same SKU,
            // but if we are, then we want to make sure that they are all the same type!
            if (matches.Select(x => (string)x["type"]).Distinct().Count() > 1)
            {
                var all = matches.Select(match => $"Parent: {match["parent_id"]} Id: {match["id"]} Sku: {match["sku"]}").ToList();
                throw new IntegrationProcessingException($"Produkti imajo na spletni trgovini isto SKU kodo, vendar različen tip produkta! Tega ne dovoljujemo! Detajli: {string.Join(",", all)}");
            }

        }
    }
}
