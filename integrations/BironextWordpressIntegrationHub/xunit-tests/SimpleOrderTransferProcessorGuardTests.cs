using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using validator;
using validator.logic;
using validator.logic.order_transfer.guard;
using validator.logic.order_transfer_processor;
using Xunit;

namespace xunit_tests {

    public class SimpleOrderTransferProcessorGuardTests {
        [Fact]
        public void Test1() {

            SimpleOrderTransferProcessorGuard guard = new SimpleOrderTransferProcessorGuard(
                new NopOrderTransferProcessorStage(),
                null,
                new NopSleepOperation());

            Assert.Equal(1, 1);
        }

        // SimpleOrderTransferProcessorGuardTests

        // HANDLE THROUGH VALIDATOR
        // if order transfer is null, should throw nullargumentexception
        // if order transfer status is unaccepted, sleeper should get called
        // if order transfer status changes on fetch, next should not get called
        // if order transfer status changes after sleep, next should not get called

        // HANDLE THROUGH CONTROLLER
        // if order transfer is null or empty, should throw nullargumentexception
        // TEST THE DESERIALIZATION!
        // if order transfer status is not unaccepted, handler should not get called


        // UnacceptedAcceptedOrderTransferProcessorStage

        

        
    }

    class NopSleepOperation : ISleepOperation {
        public async Task Sleep() {}
    }

    class MockOrderTransferAccessor : IOrderTransferAccessor {

        public MockOrderTransferAccessor() {
            Dictionary<string, OrderTransfer> map = new Dictionary<string, OrderTransfer>() {
                { "1:processing"}
            };
        }

        public OrderTransfer Get(string orderid, string orderstatus) {
            throw new NotImplementedException();
        }

        public List<OrderTransfer> GetByStatus(List<OrderTransferStatus> statuses) {
            throw new NotImplementedException();
        }

        public string GetOrder(string id) {
            throw new NotImplementedException();
        }

        public void Set(OrderTransfer orderTransfer) {
            throw new NotImplementedException();
        }
    }
}
