using ApiClient.utils;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub;

namespace BironextWordpressIntegrationHub
{
    public class OrderAdditionalParams { 
        public BirokratDocumentType SourceDocumentType { get; set; }
        public IDocumentNumberGetter SourceDocumentNumberExtractor { get; set; }
        public ICountryMapper CountryMapper { get; set; }
        public string AdditionalNumber { get; set; }
        public string ExternalUniqueIdentifier { get; set; }
        public string BirokratId { get; set; }

        public static OrderAdditionalParams Default(WoocommerceOrder order) {
            return new OrderAdditionalParams() {
                CountryMapper = null,
                AdditionalNumber = order.Data.Number,
                ExternalUniqueIdentifier = order.Data.Number,
                SourceDocumentType = BirokratDocumentType.UNASSIGNED,
                SourceDocumentNumberExtractor = null
            };
        }

        public static OrderAdditionalParams BuildDefaultWithCountryMapper(ICountryMapper countryMapper, WoocommerceOrder order) {
            var x = Default(order);
            x.CountryMapper = countryMapper;
            return x;
        }
    }

}
