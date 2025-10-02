using BironextWordpressIntegrationHub.structs;
using birowoo_exceptions;
using BiroWooHub.logic.integration;
using common_birowoo;
using core.customers;
using core.logic.common_exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using si.birokrat.next.common.build;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using tests.tools;
using tests.tools.fixture_setup;
using validator;
using validator.logic;

namespace tests.tests.estrada
{

    public class OrderTests : ITests<string>
    {

        LazyIntegration integ;
        IProgressKeeperFactory progressKeeperFactory;
        IOrderArranger arranger;
        IOrderActStage orderActStages;
        OrderAsserter asserter;
        IOutcomeHandler errorHandler;

        SimpleDecoratingFactory<IIntegration, IOutcomeHandler> decorator;

        public OrderTests(LazyIntegration integ,
            IProgressKeeperFactory progressKeeperFactory,
            IOrderArranger arranger,
            IOrderActStage orderActStages,
            OrderAsserter asserter,
            SimpleDecoratingFactory<IIntegration, IOutcomeHandler> decorator) {
            if (integ == null)
                throw new ArgumentNullException("integ");
            if (progressKeeperFactory == null)
                throw new ArgumentNullException("progressKeeper");
            if (orderActStages == null)
                throw new ArgumentNullException("orderActStages");
            if (asserter == null)
                throw new ArgumentNullException("asserter");
            if (arranger == null)
                throw new ArgumentNullException("arranger");
            this.integ = integ;
            this.progressKeeperFactory = progressKeeperFactory;
            this.orderActStages = orderActStages;
            this.asserter = asserter;
            this.arranger = arranger;
            this.decorator = decorator;


            
        }

        IProgressKeeper progressKeeper;

        public async Task Work(CancellationToken token) {

            IIntegration integr = await integ.BuildIntegrationAsync.Invoke();
            List<string> orders = await arranger.Arrange(integr);
            progressKeeper = progressKeeperFactory.Create(integr);
            this.errorHandler = new ProgressKeeperOutcomeHandler(progressKeeper);
            errorHandler = decorator.Decorate(integr, errorHandler);
            errorHandler = new RestoreOriginalOrderNumbers(errorHandler);

            foreach (string originalOrder1 in orders) {

                if (token.IsCancellationRequested) {
                    token.ThrowIfCancellationRequested();
                }

                string originalOrder = originalOrder1;
                var modifiedOrder = JsonConvert.DeserializeObject<WoocommerceOrder>(originalOrder);
                
                var context = new Dictionary<string, object>();
                context["integrationName"] = integ.Name;



                try
                {
                    var results = await orderActStages.Act(integr, modifiedOrder);
                    await errorHandler.HandleSuccess(context, originalOrder, results);
                    modifiedOrder.Data.Number = (string)results["orderNumber"];

                    string tmp = Path.Combine(integr.Datafolder, modifiedOrder.Data.Number + ".json");
                    File.WriteAllText(tmp, originalOrder);

                    // assert
                    var comparisonResult = await asserter.Assert(integr, modifiedOrder);
                    if (!comparisonResult.Success)
                    {
                        await errorHandler.Handle(context, originalOrder, comparisonResult.ErrorType.ToString(), new Exception(comparisonResult.ErrorMessage));
                    }
                    else
                    {
                        await errorHandler.HandleVerified(context, originalOrder);
                    }
                }
                catch (InconsistentWoocommerceOrderPrices ex)
                {
                    await errorHandler.Handle(context, originalOrder, "InconsistentWoocommerceOrderPricesException", ex);
                }
                catch (IntegrationProcessingException ex)
                {
                    await errorHandler.Handle(context, originalOrder, $"IntegrationProcessingException", ex);
                }
                catch (Exception ex)
                {
                    await errorHandler.Handle(context, originalOrder, $"Unknown exception", ex);
                }

            }
        }

        public string GetResult() {
            // needless coupling with tests_gui.MainForm - overfitter for there
            // will use an abstraction if we get another use case
            
            string tmp = progressKeeper.ToString();
            List<string> result = new List<string>();
            var array = JsonConvert.DeserializeObject<List<ProgressState>>(tmp);
            foreach (var x in array) {
                if (x.message == "Successfully finished!")
                {
                    result.Add("<green>");
                }
                else if (x.message == "TaxOneCentException") {
                    result.Add("<yellow>");
                }
                else if (x.message == "Unknown exception")
                {
                    result.Add("<red>");
                }
                else
                {
                    result.Add("<orange>");
                }
                result.Add(JsonConvert.SerializeObject(x));
            }
            return string.Join("\n", result);
        }
    }


    class RestoreOriginalOrderNumbers : IOutcomeHandler
    {
        IOutcomeHandler next;
        public RestoreOriginalOrderNumbers(IOutcomeHandler next) {
            this.next = next;
        }
        public async Task Handle(Dictionary<string, object> context, string originalOrder, string message, Exception ex = null)
        {
            await next.Handle(context, OriginalOrderNumberRestorer.EnrichWithOriginalMetadata(originalOrder), message, ex);
        }

        public async Task HandleSuccess(Dictionary<string, object> context, string originalOrder, Dictionary<string, object> results)
        {
            await next.HandleSuccess(context, OriginalOrderNumberRestorer.EnrichWithOriginalMetadata(originalOrder), results);
        }

        public async Task HandleVerified(Dictionary<string, object> context, string originalOrder)
        {
            await next.HandleVerified(context, OriginalOrderNumberRestorer.EnrichWithOriginalMetadata(originalOrder));
        }

        
    }

    class OriginalOrderNumberRestorer {
        public static string EnrichWithOriginalMetadata(string originalOrder)
        {
            // first restore original number and id so that we know which order this is about in the first place!
            var od = JsonConvert.DeserializeObject<WoocommerceOrder>(originalOrder);
            string origi_number = (string)od.Data.MetaData.Where(x => x.Key == "original_number").Single().Value;
            string origi_id = (string)od.Data.MetaData.Where(x => x.Key == "original_id").Single().Value;
            od.Data.Number = origi_number + "_____" + od.Data.Number;
            originalOrder = JsonConvert.SerializeObject(od);
            return originalOrder;
        }
    }

    public class OrderTransferOutcomeHandler : IOutcomeHandler
    {
        private IOutcomeHandler next;
        private OrderTransferManager manager;

        public OrderTransferOutcomeHandler(IOutcomeHandler next)
        {
            this.next = next;
            this.manager = new OrderTransferManager();
        }

        public async Task Handle(Dictionary<string, object> context, string originalOrder, string message, Exception ex = null)
        {
            var ot = manager.Handle(originalOrder, message, ex);
            await next.Handle(context, originalOrder, message, ex);
        }

        public async Task HandleSuccess(Dictionary<string, object> context, string originalOrder, Dictionary<string, object> results)
        {
            var ot = manager.HandleSuccess(originalOrder, results);
            await next.HandleSuccess(context, originalOrder, results);
        }

        public async Task HandleVerified(Dictionary<string, object> context, string originalOrder)
        {
            var ot = manager.HandleVerified(originalOrder);
            await next.HandleVerified(context, originalOrder);
        }

        public Dictionary<string, OrderTransfer> Get()
        {
            return manager.Get();
        }
    }

    public class OrderTransferManager
    {
        private Dictionary<string, OrderTransfer> orderTransfers;

        public OrderTransferManager()
        {
            this.orderTransfers = new Dictionary<string, OrderTransfer>();
        }

        public OrderTransfer Handle(string originalOrder, string message, Exception ex = null)
        {
            var od = JsonConvert.DeserializeObject<WoocommerceOrder>(originalOrder);
            var signature = od.Data.Status + od.Data.Number;
            var ot = new OrderTransfer();

            if (orderTransfers.ContainsKey(signature))
            {
                ot = orderTransfers[signature];
            }
            else
            {
                ot.OrderId = od.Data.Id.ToString();
                ot.OrderStatus = od.Data.Status;
            }

            ot.OrderId = "Id=" + od.Data.Id + "_Number=" + od.Data.Number.ToString() + od.Data.Status;
            ot.Error = message + ex?.Message ?? "";
            ot.OrderTransferStatus = OrderTransferStatus.ERROR;
            orderTransfers[signature] = ot;

            return ot;
        }

        public OrderTransfer HandleSuccess(string originalOrder, Dictionary<string, object> results)
        {
            var od = JsonConvert.DeserializeObject<WoocommerceOrder>(originalOrder);
            var signature = od.Data.Status + od.Data.Number;

            var ot = new OrderTransfer();
            ot.DateCreated = DateTime.Now;
            ot.OrderId = "Id=" + od.Data.Id + "_Number=" + od.Data.Number.ToString();
            ot.OrderStatus = od.Data.Status;
            ot.OrderTransferStatus = OrderTransferStatus.UNVERIFIED;
            ot = new OrderTransferBuilder().Success(ot, results);
            orderTransfers[signature] = ot;

            return ot;
        }

        public OrderTransfer HandleVerified(string originalOrder)
        {
            var od = JsonConvert.DeserializeObject<WoocommerceOrder>(originalOrder);
            var signature = od.Data.Status + od.Data.Number;

            var ot = orderTransfers[signature];
            ot.OrderTransferStatus = OrderTransferStatus.VERIFIED;
            ot.OrderId = "Id=" + od.Data.Id + "_Number=" + od.Data.Number.ToString();

            return ot;
        }

        public Dictionary<string, OrderTransfer> Get()
        {
            return orderTransfers;
        }
    }

    public class ProgressKeeperOutcomeHandler : IOutcomeHandler
    {
        private readonly IProgressKeeper _progressKeeper;

        public ProgressKeeperOutcomeHandler(IProgressKeeper progressKeeper)
        {
            _progressKeeper = progressKeeper;
        }

        public async Task HandleSuccess(Dictionary<string, object> context, string originalOrder, Dictionary<string, object> results)
        {
            await Handle(context, originalOrder, "Successfully finished!");
        }

        public async Task Handle(Dictionary<string, object> context, string originalOrder, string message, Exception ex = null)
        {
            Console.WriteLine(message);

            string error = "";
            if (ex != null)
            {
                error = ex.Message + ex.StackTrace;
            }

            string some = JsonConvert.DeserializeObject<WoocommerceOrder>(originalOrder).Data.Id + "";

            _progressKeeper.SaveState(new ProgressState()
            {
                signature = originalOrder,
                message = message,
                error = error,
                additionalinfo = some
            });
        }

        public async Task HandleVerified(Dictionary<string, object> context, string originalOrder)
        {
            await Handle(context, originalOrder, "Successfully finished!");
        }
    }
}
