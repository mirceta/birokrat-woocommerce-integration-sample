using birowoo_exceptions;
using System;

namespace core.logic.mapping_biro_to_woo
{
    public class ProductUpdatingException : IntegrationProcessingException {
        public ProductUpdatingException(string message) : base(message) { }
        public ProductUpdatingException(string message, Exception inner) : base(message, inner) { }
    }
}
