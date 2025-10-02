using BiroWoocommerceHubTests;
using Newtonsoft.Json;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using transfer_data_abstractions.orders;
using validator;
using validator.logic.order_transfer.accessor;

namespace transfer_data.orders.accessor
{

    internal class WoocommerceWebshopOrderTransferAccessor : IOrderTransferAccessor
    {

        IOutApiClient wooclient;

        public WoocommerceWebshopOrderTransferAccessor(IOutApiClient wooclient)
        {
            this.wooclient = wooclient;
        }

        public async Task<List<OrderTransfer>> GetByStatus(List<OrderTransferStatus> statuses)
        {

            string tmp = await GetOrderTransfers();

            List<OrderTransfer> orderTransfers = JsonConvert.DeserializeObject<List<OrderTransferJson>>(tmp).Select(x => new OrderTransfer(x)).ToList();
            return orderTransfers;
        }

        public async Task<OrderTransfer> Get(string orderid, string orderstatus)
        {

            string tmp = await GetOrderTransfer(JsonConvert.SerializeObject(new { orderid, orderstatus }));

            List<OrderTransfer> orderTransfers = JsonConvert.DeserializeObject<List<OrderTransferJson>>(tmp).Select(x => new OrderTransfer(x)).ToList();
            return orderTransfers.Where(x => x.OrderStatus == orderstatus && x.OrderId == orderid).First();
        }

        public async Task<string> GetOrder(string id)
        {
            return await new WooCustomOrderRetriever(wooclient).GetOrder(id);
        }

        public async Task AddUnaccepted(string orderid, string orderstatus)
        {

            string result = AddUnacceptedOrderTransfer(orderid, orderstatus).GetAwaiter().GetResult();
        }

        public async Task Delete(string orderid, string orderstatus)
        {

            string some = await DeleteOrderTransfer(orderid, orderstatus);
        }

        public async Task Set(OrderTransfer orderTransfer)
        {
            await PutOrderTransfer(orderTransfer);
        }

        private async Task<string> PutOrderTransfer(OrderTransfer orderTransfer)
        {
            var sm = orderTransfer.ToJson();

            Dictionary<string, string> map = new Dictionary<string, string>() {
                { "orderid", sm.orderid},
                { "orderstatus", sm.orderstatus},
                { "ordertransferstatus", sm.ordertransferstatus},
                { "birokratdoctype", sm.birokratdoctype},
                { "birokratdocnum", sm.birokratdocnum},
                { "datelastmodified", sm.datelastmodified},
                { "datevalidated", sm.datevalidated},
            };
            map = map.Where(x => !string.IsNullOrEmpty(map[x.Key])).ToDictionary(x => x.Key, x => x.Value);

            // exception: when sending error, it's possible to update it with null or empty string
            if (sm.error != null && sm.error.Length > 0)
            {
                sm.error = sm.error.Substring(0, Math.Min(299, sm.error.Length));
            }
            else if (sm.error == null) sm.error = "";
            map["error"] = sm.error;

            string query = $"my_ordertransfer/set?";
            var keylist = map.Keys.ToList();

            for (int i = 0; i < keylist.Count; i++)
            {

                if (keylist[i] == "error")
                {
                    // tried preserving whitespace but kept saying that the string is not terminated then
                    string tmp = string.Concat(map[keylist[i]].Where((x) => char.IsWhiteSpace(x) || char.IsLetterOrDigit(x)));
                    tmp = tmp.Replace(" ", "_").Replace("\n", "_").Replace("\t", "_").Replace("\r", "_");
                    map[keylist[i]] = tmp;
                }

                query += $"{keylist[i]}={map[keylist[i]]}";
                if (i < keylist.Count - 1)
                    query += "\'&\'";
            }

            // WARNING: FOR SOME REASON, query param named error cannot be passed to woocommerce rest api
            query = query.Replace("error=", "except=");
            string result = await wooclient.PutKita(query,
                JsonConvert.SerializeObject(new { orderid = orderTransfer.OrderId, orderstatus = orderTransfer.OrderStatus }));

            if (result.ToLower().Trim() != "true")
            {
                throw new Exception("Could not set order transfer!");
            }
            return result;
        }

        public async Task<string> GetOrderTransfers()
        {
            string tmp = await wooclient.GetKita("my_ordertransfer/get");
            return tmp;
        }

        public async Task<string> GetOrderTransfer(string orderTransfer)
        {
            string tmp = await wooclient.PostKita("my_ordertransfer/get", orderTransfer);
            return tmp;
        }

        public async Task<string> AddUnacceptedOrderTransfer(string orderid, string orderstatus)
        {
            return await wooclient.PutKita($"my_ordertransfer/unaccepted?orderid={orderid}\'&\'orderstatus={orderstatus}", "{}");
        }

        public async Task<string> DeleteOrderTransfer(string orderid, string orderstatus)
        {
            string some = await wooclient.DeleteKita($"my_ordertransfer/delete?orderid={orderid}\'&\'orderstatus={orderstatus}");
            return some;
        }
    }

    public class LoggerOrderTransferAccessor : IOrderTransferAccessor
    {
        private readonly IOrderTransferAccessor next;
        private readonly IMyLogger logger; // assuming a logger interface

        public LoggerOrderTransferAccessor(IMyLogger logger, IOrderTransferAccessor next)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task AddUnaccepted(string orderid, string orderstatus)
        {
            await next.AddUnaccepted(orderid, orderstatus);
        }

        public async Task Delete(string orderid, string orderstatus)
        {
            await next.Delete(orderid, orderstatus);
        }

        public async Task<OrderTransfer> Get(string orderid, string orderstatus)
        {
            return await next.Get(orderid, orderstatus);
        }

        public async Task<List<OrderTransfer>> GetByStatus(List<OrderTransferStatus> statuses)
        {
            return await next.GetByStatus(statuses);
        }

        public async Task<string> GetOrder(string id)
        {
            return await next.GetOrder(id);
        }

        public async Task Set(OrderTransfer orderTransfer)
        {
            logger.LogInformation(orderTransfer.ToString());
            await next.Set(orderTransfer);
        }
    }
}