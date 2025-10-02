using core.tools.wooops;
using si.birokrat.next.common.logging;
using System;
using System.Threading.Tasks;
using webshop_client_woocommerce.woo_caller;

namespace BiroWoocommerceHubTests
{
    public class WoocommerceCaller_NetworkFailureGuard : IWooApiCaller {

        IWooApiCaller next;
        int retryCount;

        public WoocommerceCaller_NetworkFailureGuard(int retryCount, IWooApiCaller next) {
            this.next = next;
            this.retryCount = retryCount;
        }

        public string Ck => next.Ck;

        public string Cs => next.Cs;

        public string Address => next.Address;

        public string Version => next.Version;

        public async Task<string> Delete(string op) {
            return await callWithRetries(async () => (await next.Delete(op)).Trim().Replace("﻿", ""));
        }

        public async Task<string> Get(string op) {
            return await callWithRetries(async () => (await next.Get(op)).Trim().Replace("﻿", ""));
        }

        public async Task<string> Post(string op, string body) {
            return await callWithRetries(async () => (await next.Post(op, body)).Trim().Replace("﻿", ""));
        }

        public async Task<string> Put(string op, string body) {
            return await callWithRetries(async () => (await next.Put(op, body)).Trim().Replace("﻿", ""));
        }

        public void SetLogger(IMyLogger logger) {
            next.SetLogger(logger);
        }

        private async Task<string> callWithRetries(Func<Task<string>> op) {
            string json = "";
            for (int i = 0; i < retryCount; i++) {
                json = await op();
                object some = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<object>(json);
                if (json.Length > 20 && json.Substring(0, 20).Contains("Traceback")) {
                    continue;
                }
                return json;
            }
            throw new Exception("WoocommerceCaller_NetworkFailureGuard: " + json);
        }
    }
}
