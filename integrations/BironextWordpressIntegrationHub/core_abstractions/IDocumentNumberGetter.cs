using BironextWordpressIntegrationHub.structs;
using core.logic.mapping_woo_to_biro.document_insertion;
using System.Threading.Tasks;

namespace BironextWordpressIntegrationHub {
    public interface IDocumentNumberGetter {
        Task<DocumentNumberResult> GetDocumentNumber(string birokratDocumentType, WoocommerceOrder order);
    }
    public class DocumentNumberResult
    {
        public bool Success { get; set; }
        public string DocumentNumber { get; set; }
        public string ErrorMessage { get; set; }

        public static DocumentNumberResult SuccessResult(string documentNumber)
        {
            return new DocumentNumberResult { Success = true, DocumentNumber = documentNumber };
        }

        public static DocumentNumberResult FailureResult(string errorMessage)
        {
            return new DocumentNumberResult { Success = false, ErrorMessage = errorMessage };
        }
    }
}
