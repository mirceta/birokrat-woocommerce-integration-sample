using biro_to_woo.logic.change_trackers.exhaustive;
using BiroWoocommerceHubTests;
using common_abstractions_core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace tests_fixture
{
    internal class OrderRetrieverModeFactory
    {
        ILogger logger;
        string mode;
        public OrderRetrieverModeFactory(ILogger logger, string mode = "ids") {
            this.logger = logger;
            this.mode = mode;
        }

        public IAsyncOperation Create(IOutApiClient woo, int page, ConcurrentBag<OrderDescription> accumulator) {
            if (mode == "ids")
            {
                RetrieveOnePageOfOrderIds ctx = new RetrieveOnePageOfOrderIds(woo, page, accumulator);
                return new RetryingAsyncOperation(ctx, new DotnetStandardLoggerWrapper(logger));
            }
            else {
                throw new Exception("This mode does not exist");
            }
        }
    }
}
