using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tests.tools;

namespace tests.tests.estrada
{

    public interface IOrderArranger
    {
        Task<List<string>> Arrange(IIntegration integ);
    }

    public class OrderArranger : IOrderArranger
    {

        List<ISetupStage> setup;
        IOrderStore store;
        bool checkBirokratNastavitveValid;

        public OrderArranger(List<ISetupStage> setup,
            IOrderStore store,
            bool checkBirokratNastavitveValid) {
            this.setup = setup;
            this.store = store;
            this.checkBirokratNastavitveValid = checkBirokratNastavitveValid;
        }

        public async Task<List<string>> Arrange(IIntegration integ) {
            foreach (var stage in setup) {
                await stage.Work();
            }

            IIntegration integr = integ;
            if (checkBirokratNastavitveValid)
                await integr.ObvezneNastavitve.Verify(integ.BiroClient);

            var orders = await store.GetOrders();
            return orders;
        }
    }

    public class OrderStatusModifier : IOrderArranger
    {
        /*
            Foreach value in statusesToOperate on, each order will be multipled and changed to that status.
            If statusesToOperateOn is "on-hold", "completed", then order 1,"anystatus" will become (order 1,"onhold"), (order 1, "completed")
            in the testset
         */

        IOrderArranger next;
        List<string> statusesToOperateOn;
        public OrderStatusModifier(List<string> statusesToOperateOn, IOrderArranger next) {
            this.next = next;
            this.statusesToOperateOn = statusesToOperateOn;
        }

        public async Task<List<string>> Arrange(IIntegration integ)
        {
            var orders = await next.Arrange(integ);
            List<string> newOrders = new List<string>();
            foreach (var order in orders) {

                var tmp = new JsonPowerDeserialization2().DeserializeObjectImmuneToBadJSONEscapeSequenece<WoocommerceOrder>(order);


                // WARNING: The number should not be modified, because for example if we want to connect a dobavnica to a racun - where both
                //          are created from the same order,
                //          then they will not be connectable if the number is changed!
                foreach (var x in statusesToOperateOn) {
                    tmp.Data.Status = x;
                    tmp.Data.Number = tmp.Data.Number;
                    tmp.Data.Id = tmp.Data.Id;
                    newOrders.Add(JsonConvert.SerializeObject(tmp));
                }
            }
            return newOrders;
        }
    }

    public class ModifyOrderForTestingArranger : IOrderArranger
    {
        int sessionId;
        IOrderArranger next;
        public ModifyOrderForTestingArranger(int sessionId, IOrderArranger next)
        {
            this.sessionId = sessionId;
            this.next = next;
        }
        public async Task<List<string>> Arrange(IIntegration integration)
        {
            var orders = await next.Arrange(integration);
            List<string> result = new List<string>();
            foreach (var orderstr in orders) {
                var order = JsonConvert.DeserializeObject<WoocommerceOrder>(orderstr);
                order.Data.MetaData.Add(new MetaData() { Id = 100000, Key = "original_number", Value = order.Data.Number });
                order.Data.MetaData.Add(new MetaData() { Id = 100000, Key = "original_id", Value = order.Data.Id + "" });
                order = TestUtils.ModifyOrderForTesting(JsonConvert.SerializeObject(order), sessionId);
                result.Add(JsonConvert.SerializeObject(order));
            }
            return result;
        }
    }

    public class FilterProcessedOrders : IOrderArranger
    {

        IOrderArranger next;
        IProgressKeeperFactory progressKeeperFactory;
        public FilterProcessedOrders(IProgressKeeperFactory progressKeeperFactory, IOrderArranger next) {
            this.next = next;
            this.progressKeeperFactory = progressKeeperFactory;
        }

        public async Task<List<string>> Arrange(IIntegration integ)
        {
            var orders = await next.Arrange(integ);
            var progressKeeper = progressKeeperFactory.Create(integ);
            progressKeeper.Setup();
            orders = orders.Where(x => !progressKeeper.IsAlreadyProcessed(x)).ToList();
            return orders;
        }
    }
}
