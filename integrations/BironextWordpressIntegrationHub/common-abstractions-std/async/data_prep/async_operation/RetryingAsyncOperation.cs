using common_abstractions_std.wrappers;
using System;

namespace biro_to_woo.logic.change_trackers.exhaustive
{
    public class RetryingAsyncOperation : IAsyncOperation {
        IAsyncOperation context;
        ILogger logger;
        public RetryingAsyncOperation(IAsyncOperation context, ILogger logger) {
            this.context = context;
            this.logger = logger;
        }

        public void Work() {
            for (int i = 0; i < 5; i++) {
                try {
                    context.Work();
                    break;
                } catch (Exception ex) {
                    logger.LogWarning($"Retrying async context {i} {ex.Message} {ex.StackTrace}");
                }
            }
        }
    }
}
