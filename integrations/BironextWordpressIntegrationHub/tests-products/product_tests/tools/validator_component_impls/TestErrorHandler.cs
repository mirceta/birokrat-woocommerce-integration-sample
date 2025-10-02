using biro_to_woo_common.error_handling.errors;
using biro_to_woo_common.error_handling.reports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests.tests.hisavizij {
    public class TestErrorHandler : IErrorHandler {


        List<IOperationReport> reports;
        IErrorHandler next;
        public TestErrorHandler(IErrorHandler next) {
            reports = new List<IOperationReport>();
            this.next = next;
        }
        
        public async Task HandleError(IOperationReport report) {
            reports.Add(report);
            await next.HandleError(report);
        }

        public async Task HandleError(string signature, Exception ex) {
            await next.HandleError(signature, ex);
        }

        public async Task HandleErrorList(List<IOperationReport> reports) {
            await next.HandleErrorList(reports);    
        }

        public async void Reset() {
            reports = new List<IOperationReport>();
        }

        public List<IOperationReport> GetReports() {
            return reports;
        }
    }
}
