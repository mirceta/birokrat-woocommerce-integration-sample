using core.structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace biro_to_woo_common.executor.validation_stages.validators
{
    internal class ProductTransferVerifyOperationHelper
    {
        public static void nullGuards(string sku, BiroOutComparisonContext context) {
            if (string.IsNullOrEmpty(sku))
            {
                throw new ArgumentNullException("sku");
            }
            if (context == null || context.biroItems == null || context.outItems == null)
            {
                throw new ArgumentNullException("context");
            }
        }
    }
}
