using ApiClient.utils;
using birowoo_exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using transfer_data_abstractions.orders;
using validator;

namespace tests.tools.order_transfer_tests
{
    public class WebshopAccessTests {

        IOrderTransferAccessor accessor;
        public WebshopAccessTests(IOrderTransferAccessor accessor) {
            this.accessor = accessor;
        }

        // Currently needs no progress tracking
        public async Task OrderTransfer_Tests() {
            var ts = OrderTransferTestset.get();
            List<Tuple<string, string>> ots = GetDistinctOrderTransfers(ts);
            foreach (var ot in ots)
                await accessor.AddUnaccepted(ot.Item1, ot.Item2);

            foreach (var ot in ts) {
                await accessor.Set(ot);
                var tmpot = await accessor.Get(ot.OrderId, ot.OrderStatus);
                AssertEqual(ot, tmpot);
            }
        }

        private static List<Tuple<string, string>> GetDistinctOrderTransfers(List<OrderTransfer> ts) {
            return ts.Select(x => new Tuple<string, string>(x.OrderId, x.OrderStatus)).Distinct().ToList();
        }

        public void Cleanup() {
            // TODO need to delete the orders that were included in the tests!
            var ts = OrderTransferTestset.get();
            List<Tuple<string, string>> ots = GetDistinctOrderTransfers(ts);
            foreach (var ot in ots) {
                accessor.Delete(ot.Item1, ot.Item2);
                try {
                    accessor.Get(ot.Item1, ot.Item2);
                    throw new Exception("Order transfer should have been deleted!");
                } catch (InvalidOperationException ex) {
                    // ok
                }
                
            }
        }

        public void GetOrder_Tests() {
            // currently not needed, because to test the main business logic we get the 
            // webshop orders and store them as test cases.
        }

        private void AssertEqual(OrderTransfer ot1, OrderTransfer ot2) {
            if (ot1.OrderId != ot2.OrderId) {
                throw new OrderTransferValidationException("OrderId");
            }
            if (ot1.OrderStatus != ot2.OrderStatus) {
                throw new OrderTransferValidationException("OrderStatus");
            }
            if (ot1.OrderTransferStatus != ot2.OrderTransferStatus) {
                throw new OrderTransferValidationException("OrderTransferStatus");
            }
            ot1.Error = String.Concat(ot1.Error.
                Where((x) => char.IsLetterOrDigit(x)));
            if (!(ot1.Error == null && ot2.Error == "") &&
                !(ot1.Error == "" && ot2.Error == null) && 
                ot1.Error != ot2.Error) {
                throw new OrderTransferValidationException("Error");
            }
            if (ot1.BirokratDocType != ot2.BirokratDocType) {
                throw new OrderTransferValidationException("BirokratDocType");
            }
            if (!(ot1.BirokratDocNum == null && ot2.BirokratDocNum == "") &&
                !(ot1.BirokratDocNum == "" && ot2.BirokratDocNum == null) &&
                ot1.BirokratDocNum != ot2.BirokratDocNum) {
                throw new OrderTransferValidationException("BirokratDocNum");
            }
        }


    }

    public class OrderTransferTestset {
        public static List<OrderTransfer> get() {
            var ots = new List<OrderTransfer>();
            
            ots.Add(new OrderTransfer() {
                OrderId = "1",
                OrderStatus = "processing",
                OrderTransferStatus = OrderTransferStatus.ERROR,
                BirokratDocType = BirokratDocumentType.UNASSIGNED,
                BirokratDocNum = "",
                Error = @"problem",
                DateCreated = DateTime.Now,
                DateLastModified = DateTime.Now,
                DateValidated = DateTime.Now
            });
            ots.Add(new OrderTransfer() {
                OrderId = "1",
                OrderStatus = "processing",
                OrderTransferStatus = OrderTransferStatus.UNACCEPTED,
                BirokratDocType = BirokratDocumentType.UNASSIGNED,
                BirokratDocNum = "",
                Error = "",
                DateCreated = DateTime.Now,
                DateLastModified = DateTime.Now,
                DateValidated = DateTime.Now
            });
            ots.Add(new OrderTransfer() {
                OrderId = "1",
                OrderStatus = "processing",
                OrderTransferStatus = OrderTransferStatus.ACCEPTED,
                BirokratDocType = BirokratDocumentType.UNASSIGNED,
                BirokratDocNum = "",
                Error = "",
                DateCreated = DateTime.Now,
                DateLastModified = DateTime.Now,
                DateValidated = DateTime.Now
            });
            
            ots.Add(new OrderTransfer() {
                OrderId = "1",
                OrderStatus = "processing",
                OrderTransferStatus = OrderTransferStatus.NO_EVENT,
                BirokratDocType = BirokratDocumentType.UNASSIGNED,
                BirokratDocNum = "",
                Error = "",
                DateCreated = DateTime.Now,
                DateLastModified = DateTime.Now,
                DateValidated = DateTime.Now
            });
            ots.Add(new OrderTransfer() {
                OrderId = "1",
                OrderStatus = "processing",
                OrderTransferStatus = OrderTransferStatus.UNVERIFIED,
                BirokratDocType = BirokratDocumentType.RACUN,
                BirokratDocNum = "000001",
                Error = "",
                DateCreated = DateTime.Now,
                DateLastModified = DateTime.Now,
                DateValidated = DateTime.Now
            });
            ots.Add(new OrderTransfer() {
                OrderId = "1",
                OrderStatus = "processing",
                OrderTransferStatus = OrderTransferStatus.VERIFIED,
                BirokratDocType = BirokratDocumentType.RACUN,
                BirokratDocNum = "1",
                Error = "",
                DateCreated = DateTime.Now,
                DateLastModified = DateTime.Now,
                DateValidated = DateTime.Now
            });
            ots.Add(new OrderTransfer() {
                OrderId = "1",
                OrderStatus = "processing",
                OrderTransferStatus = OrderTransferStatus.VERIFICATION_ERROR,
                BirokratDocType = BirokratDocumentType.RACUN,
                BirokratDocNum = "1",
                Error = "Verification error: The number of the order and the racun number were inconsistent",
                DateCreated = DateTime.Now,
                DateLastModified = DateTime.Now,
                DateValidated = DateTime.Now
            });
            return ots;
        }
    }
}
