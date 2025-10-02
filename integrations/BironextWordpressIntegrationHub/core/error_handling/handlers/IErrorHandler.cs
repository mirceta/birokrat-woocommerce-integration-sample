using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using biro_to_woo_common.error_handling.reports;

namespace biro_to_woo_common.error_handling.errors
{
    public interface IErrorHandler
    {
        Task HandleErrorList(List<IOperationReport> reports);
        Task HandleError(IOperationReport report);
        Task HandleError(string signature, Exception ex);
    }
}