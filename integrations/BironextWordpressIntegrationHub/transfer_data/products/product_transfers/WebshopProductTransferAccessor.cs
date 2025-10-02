using BiroWoocommerceHubTests;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace tests_webshop.products
{
    public class WebshopProductTransferAccessor : IProductTransferAccessor
    {

        IOutApiClient outclient;

        public WebshopProductTransferAccessor(IOutApiClient wooclient) {
            this.outclient = wooclient;
        }

        public async Task AddOrUpdate(ProductTransfer pt) {
            await PostOrPutProductTransfer(pt);
        }

        public async void Delete(string productid)
        {
            await DeleteProductTransfer(productid);
        }

        public async Task<List<ProductTransfer>> List() {
            return await ListProductTransfers();
        }


        public async Task<string> PostOrPutProductTransfer(ProductTransfer pt)
        {
            Dictionary<string, string> map = new Dictionary<string, string>() {
                { "product_id", pt.product_id},
                { "last_event", pt.last_event + ""},
                { "last_event_success", pt.last_event_success + ""},
                { "last_event_message", pt.last_event_message + ""},
                { "last_event_datetime", pt.last_event_datetime + ""}
            };
            map = map.Where(x => !string.IsNullOrEmpty(map[x.Key])).ToDictionary(x => x.Key, x => x.Value);

            // exception: when sending error, it's possible to update it with null or empty string
            if (pt.last_event_message != null && pt.last_event_message.Length > 0)
            {
                pt.last_event_message = pt.last_event_message.Substring(0, Math.Min(299, pt.last_event_message.Length));
            }
            else if (pt.last_event_message == null) pt.last_event_message = "";
            map["last_event_message"] = pt.last_event_message;

            string query = $"my_producttransfer/set?";
            var keylist = map.Keys.ToList();

            for (int i = 0; i < keylist.Count; i++)
            {

                if (keylist[i] == "last_event_message")
                {
                    // tried preserving whitespace but kept saying that the string is not terminated then
                    string tmp = String.Concat(map[keylist[i]].Where((x) => char.IsWhiteSpace(x) || char.IsLetterOrDigit(x)));
                    tmp = tmp.Replace(" ", "_").Replace("\n", "_");
                    map[keylist[i]] = tmp;
                }

                query += $"{keylist[i]}={map[keylist[i]]}";
                if (i < keylist.Count - 1)
                    query += "\\\"&\\\"";
            }

            // WARNING: FOR SOME REASON, query param named error cannot be passed to woocommerce rest api

            // ordertransfer/set
            // producttransfer/set
            string result = outclient.PutKita(query, JsonConvert.SerializeObject(map)).GetAwaiter().GetResult();

            //if (result.Trim().ToLower() != "true") {
            //    throw new Exception("Could not set order transfer!");
            //}
            return result;
        }

        public async Task<List<ProductTransfer>> ListProductTransfers()
        {
            string result = await outclient.GetKita("my_producttransfer/get");
            var res = JsonConvert.DeserializeObject<List<ProductTransfer>>(result);
            return res;
        }



        public async Task DeleteProductTransfer(string productid)
        {
            string some = await outclient.DeleteKita($"my_producttransfer/delete?product_id={productid}");
        }
    }

    public class ConsolePrintProductTransferAccessor : IProductTransferAccessor
    {


        public ConsolePrintProductTransferAccessor()
        {
        }

        public async Task AddOrUpdate(ProductTransfer pt)
        {
            Console.WriteLine(pt.ToString());
        }

        public void Delete(string productid)
        {
            
        }

        public async Task<List<ProductTransfer>> List()
        {
            return null;
        }

    }

    public class LoggerProductTransferAccessor : IProductTransferAccessor
    {


        IMyLogger mylogger;
        public LoggerProductTransferAccessor(IMyLogger logger)
        {
            this.mylogger = logger;
        }

        public async Task AddOrUpdate(ProductTransfer pt)
        {
            if (mylogger != null)
                mylogger.LogInformation(pt.ToString());
        }

        public async Task<List<ProductTransfer>> List()
        {
            return null;
        }

        public void Delete(string productid)
        {
            throw new NotImplementedException();
        }
    }

    public class LoggerDecoratorProductTransferAccessor : IProductTransferAccessor
    {


        IProductTransferAccessor next;
        
        IMyLogger mylogger;
        public LoggerDecoratorProductTransferAccessor(IMyLogger logger, IProductTransferAccessor next)
        {
            this.mylogger = logger;
            this.next = next;
        }

        public async Task AddOrUpdate(ProductTransfer pt)
        {
            if (mylogger != null)
                mylogger.LogInformation(pt.ToString());
            await next.AddOrUpdate(pt);
        }

        public async Task<List<ProductTransfer>> List()
        {
            return await next.List();
        }

        public void Delete(string productid)
        {
            throw new NotImplementedException();
        }
    }
}
