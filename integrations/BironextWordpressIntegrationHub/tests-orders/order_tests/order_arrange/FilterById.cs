using BironextWordpressIntegrationHub.structs;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tests.tests.estrada
{
    public class FilterById : IOrderStore
    {

        List<string> filters;
        IOrderStore next;
        public FilterById(List<string> filters, IOrderStore next) {
            this.filters = filters;
            this.next = next;
        }

        public async Task<List<string>> GetOrders() {

            var some = await next.GetOrders();
            List<string> results = new List<string>();
            foreach (var x in some) {
                var tmp = JsonConvert.DeserializeObject<WoocommerceOrder>(x);
                if (filters.Contains(tmp.Data.Id + ""))
                    continue;
                results.Add(x);
            }
            return results;
        }
    }

    public class FilterBetweenDates : IOrderStore
    {
        DateTime minimum;
        DateTime maximum;
        IOrderStore next;
        public FilterBetweenDates(DateTime minimum, DateTime maximum, IOrderStore next) {
            this.minimum = minimum;
            this.maximum = maximum;
            this.next = next;
        }

        public async Task<List<string>> GetOrders() {

            var some = await next.GetOrders();
            List<string> results = new List<string>();
            foreach (var x in some) {
                var tmp = JsonConvert.DeserializeObject<WoocommerceOrder>(x);

                var curdate = GWooOps.ParseWooDate(tmp.Data.DateCreated.Date);
                if (minimum > curdate)
                    continue;
                if (maximum < curdate)
                    continue;
                results.Add(x);
            }
            return results;
        }
    }

    public class ReturnNewestN : IOrderStore {

        int N;
        IOrderStore store;
        public ReturnNewestN(int N, IOrderStore next) {
            this.N = N;
            this.store = next;
        }

        public async Task<List<string>> GetOrders() {
            var result = await store.GetOrders();
            if (result.Count > N) {
                result = result.OrderByDescending(x =>
                {
                    var tmp = JsonConvert.DeserializeObject<WoocommerceOrder>(x);
                    var curdate = GWooOps.ParseWooDate(tmp.Data.DateCreated.Date);
                    return curdate;
                })
                .Take(N).ToList();

            }
             return result;
        }
    }
}
