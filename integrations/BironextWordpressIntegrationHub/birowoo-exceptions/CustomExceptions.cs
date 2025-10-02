using System;
using System.Collections.Generic;
using System.Text;

namespace birowoo_exceptions {

    public class ProductTestException : Exception { 
        public ProductTestException(string message) : base(message) { }
    }
    public class WooCallFailException : Exception {
        public WooCallFailException(string message) : base(message) { }
    }

    public class VariationNotFoundException : IntegrationProcessingException {
        public VariationNotFoundException(int variationid, int originalid) : base($"Variation {variationid} not found in product id {originalid}") { }
    }

    public class ProductAddingException : IntegrationProcessingException {
        public ProductAddingException(string message) : base(message) { }
        public ProductAddingException(string message, Exception inner) : base(message, inner) { }
    }

    public class ProductUpdatingException : IntegrationProcessingException {
        public ProductUpdatingException(string message) : base(message) { }
        public ProductUpdatingException(string message, Exception inner) : base(message, inner) { }
    }

    public class IntegrationProcessingException : Exception {
        // This exception should be used when it's a configuration error pertaining to the Webshop
        // E.G. : 
        // - sku code is impropperly formatted
        // - there is no variation that fits the sold product etc
        public IntegrationProcessingException(string message) : base(message) { }
        public IntegrationProcessingException(string message, Exception inner) : base(message, inner) { }
    }

    public class CannotValidateNonSyncedProductException : Exception { 
        public CannotValidateNonSyncedProductException(string message) : base(message) { }
    }

    public class ProductInDraftStatusException : IntegrationProcessingException {
        public ProductInDraftStatusException(string message) : base(message) { }
    }

    public class DocumentNotFoundException : IntegrationProcessingException {
        public DocumentNotFoundException(string message) : base(message) { }
    }

    public class DocumentAlreadyExistsException : IntegrationProcessingException
    {
        public DocumentAlreadyExistsException(string message) : base(message) { }
    }

    public class ArtikelSyncException : IntegrationProcessingException {
        public ArtikelSyncException(string message) : base(message) { }
    }

    public class ProductNotFoundException : ArtikelSyncException {
        public ProductNotFoundException(string message): base (message) {}
    }

    public class MultipleProductVariationsWithSameSku : ArtikelSyncException {
        public MultipleProductVariationsWithSameSku(string message) : base(message) { }
    }

    public class ProductStillDifferentThanArtikelAfterUpdateException : ArtikelSyncException {
        public ProductStillDifferentThanArtikelAfterUpdateException(string message) : base(message) { }
    }

    public class NoChangesDetectedException : ArtikelSyncException {
        public NoChangesDetectedException(string message) : base(message) { 
            
        }
    }

    public class ArtikelNotFoundInStanjeZaloge : ArtikelSyncException {
        public ArtikelNotFoundInStanjeZaloge(string message) : base(message) {

        }
    }
   

    public class PriceValidationException : ValidationException {
        public PriceValidationException(string message) : base(message) { }
    }
    public class OneCentValidationException : ValidationException {
        public OneCentValidationException(string message) : base(message) { }
    }
    public class TaxOneCentValidationException : ValidationException
    {
        public TaxOneCentValidationException(string message) : base(message) { }
    }
    public class TotalOneCentValidationException : ValidationException
    {
        public TotalOneCentValidationException(string message) : base(message) { }
    }
    public class SubtotalOneCentValidationException : ValidationException
    {
        public SubtotalOneCentValidationException(string message) : base(message) { }
    }

    public class InconsistentWoocommerceOrderPrices : ValidationException {
        public InconsistentWoocommerceOrderPrices(string message) : base(message) { }
    }



    public class CountryValidationException : ValidationException {
        public CountryValidationException(string message) : base(message) { }
    }


    public class ShippingAddressValidationException : ValidationException {
        public ShippingAddressValidationException(string message) : base(message) { }
    }

    public class ValidationException : Exception {
        public ValidationException(string message) : base(message) { }
    }

    public class DavcnaValidationException : ValidationException {
        public DavcnaValidationException(string message) : base(message) { }
    }

    public class OrderTransferValidationException : Exception {
        public OrderTransferValidationException(string message) : base(message) {
        }
    }

    public class UnableToCreatePartnerServerException : IntegrationProcessingException {
        public UnableToCreatePartnerServerException(string message) : base(message) { }
    }
    public class UnableToCreatePartnerBadRequestException : IntegrationProcessingException {
        public UnableToCreatePartnerBadRequestException(string message) : base(message) { }
    }
}
