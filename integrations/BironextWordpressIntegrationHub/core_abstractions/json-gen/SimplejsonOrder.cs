using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.structs_wc_to_biro;
using System.Collections.Generic;

namespace BironextWordpressIntegrationHub {
    public class SimplejsonOrder {
        public string ExternalIdentifier;
        public string AdditionalNumber;
        public string DateCreated;
        public string SourceDocumentType;
        public string SourceDocumentNumber;
        public MBilling Billing;
        public MShipping Shipping;
        public List<BirokratPostavka> Specifications;

        public static SimplejsonOrder FromWoocommerce(WoocommerceOrder order, List<BirokratPostavka> postavke, 
            string additionalNumber, string externalUniqueIdentifier,
            string sourceDocumentType, string sourceDocumentNumber,
            string birokratId) {
            return new SimplejsonOrder() {
                ExternalIdentifier = externalUniqueIdentifier,
                AdditionalNumber = additionalNumber,
                SourceDocumentType = sourceDocumentType,
                SourceDocumentNumber = sourceDocumentNumber,
                DateCreated = order.Data.DateCreated.Date,
                Billing = new MBilling(order.Data.Billing, birokratId),
                Shipping = new MShipping(order.Data.Shipping),
                Specifications = postavke
            };
        }
    }

}
