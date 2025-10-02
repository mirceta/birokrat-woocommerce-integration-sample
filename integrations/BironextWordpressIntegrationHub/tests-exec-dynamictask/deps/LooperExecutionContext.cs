using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = tests.composition.common.ExecutionContext;

namespace tests_exec_dynamictask.deps
{
    class LooperExecutionContext
    {
        public Task t { get; set; }
        public ExecutionContext btwo { get; set; }
        public CancellationTokenSource cancellationTokenSource { get; set; }
    }
}
