using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests.tests.estrada
{
    public interface IOutcomeHandler {

        Task HandleSuccess(Dictionary<string, object> context, string originalOrder, Dictionary<string, object> results);
        Task Handle(Dictionary<string, object> context, string originalOrder, string message, Exception ex = null);
        Task HandleVerified(Dictionary<string, object> context, string originalOrder);
    }
}
