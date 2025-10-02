using BiroWoocommerceHubTests;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace webshop_client_woocommerce.woo_caller {
    public interface IWooApiCaller {
        public string Ck { get; }
        public string Cs { get; }
        public string Address { get; }
        public string Version { get; }

        public Task<string> Post(string op, string body);
        public Task<string> Put(string op, string body);
        public Task<string> Get(string op);
        public Task<string> Delete(string op);
        public void SetLogger(si.birokrat.next.common.logging.IMyLogger logger);
    }
}
