using biro_to_woo.loop;
using BiroWooHub.logic.integration;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using transfer_data.system;
using validator.logic;
using validator.logic.order_transfer;
using validator.logic.order_transfer.retrying;

namespace validator
{
    public class ValidatorSynchronization : ISynchronization {

        List<IIntegration> integs;
        IOrderTransferProcessorRootFactory orderProcessorBuilder;
        OrderTransferSystemFactory otsFactory;

        UnretriedErrosToUnaccepted errorRetrier;
        IMyLogger logger;

        public ValidatorSynchronization(List<IIntegration> integs,
            IOrderTransferProcessorRootFactory orderProcessorBuilder,
            OrderTransferSystemFactory otaFactory,
            IMyLogger logger) {
            this.integs = integs;
            this.orderProcessorBuilder = orderProcessorBuilder;
            this.otsFactory = otaFactory;
            this.logger = logger;

            errorRetrier = new UnretriedErrosToUnaccepted(5, 10, new PersistentRetryTracker("retry_tracker.txt"));
        }

        public async Task Work() {
            foreach (var integ in integs) {
                try
                {
                    await WorkSingle(integ);
                }
                catch (Exception ex) {
                    logger.LogInformation($"{integ.Name} exception caught in root. Continuing with next integration");
                }
            }
        }

        public async Task WorkSingle(IIntegration integ) {
            
            var factory = otsFactory.Get(integ); // create this better

            var creator = await factory.GetOrderTransferCreator(integ);
            await creator.CreateNewOrderTransfers();

            var accessor = await factory.GetOrderTransferAccessor(integ);

            List<OrderTransfer> orderTransfers = null;
            try {
                orderTransfers = await accessor.GetByStatus(null);
            } catch (Exception ex) {
                throw ex;
            }


            // delete order transfers that are more than a year old!
            var toDelete = orderTransfers.Where(x => DateTime.Now.Subtract(x.DateCreated).TotalDays > 100).ToList();
            foreach (var x in toDelete) {
                await accessor.Delete(x.OrderId, x.OrderStatus);
            }

            orderTransfers.RemoveAll(x => toDelete.Contains(x));


            // process only those that are not processed!
            orderTransfers = orderTransfers.Where(x => x.OrderTransferStatus != OrderTransferStatus.VERIFIED &&
                    x.OrderTransferStatus != OrderTransferStatus.VERIFICATION_ERROR).ToList();

            orderTransfers = orderTransfers.Select(x => errorRetrier.Set_UnretriedErrorOrderTransfers_toUnaccepted(integ.Name, x, accessor))
                            .Where(x => x.OrderTransferStatus != OrderTransferStatus.ERROR).ToList();      

            foreach (var ot in orderTransfers) {

                try {
                    var processor = orderProcessorBuilder.Create(integ,
                        accessor,
                        ot.OrderTransferStatus,
                        new FixDecimalsInOrder(integ.WooClient));
                    if (processor != null)
                        await processor.HandleId(ot);
                } 
                catch (NoProcessorDefinedForThisOrderTransferStatus ex) { } 
                catch (Exception ex) { }
            }
        }
    }
}
