using biro_to_woo_common.error_handling.errors;
using biro_to_woo_common.error_handling.reports;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace core.error_handling.handlers {
    public class ConsoleErrorHandler : IErrorHandler {
        public Task HandleError(IOperationReport report) {

            if (report.OperationOutcome == OperationOutcome.Ignore)
                return Task.CompletedTask;
            
            string exc = report.Ex == null ? "" : report.Ex.Message;
            Console.WriteLine(report.Id + report.Signature + report.OperationOutcome.ToString() + exc);
            return Task.CompletedTask;
        }

        public Task HandleError(string signature, Exception ex) {
            Console.WriteLine(signature + ex == null ? "" : ex.Message);
            return Task.CompletedTask;
        }

        public Task HandleErrorList(List<IOperationReport> reports) {
            foreach (var x in reports) {
                HandleError(x);
            }
            return Task.CompletedTask;
        }
    }
}
