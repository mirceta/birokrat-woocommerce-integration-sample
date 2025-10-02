using birowoo_exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace common_birowoo {
    public interface IntegrationProcessingExceptionHandler {
        Task Handle(IntegrationProcessingException ex);
    }
}
