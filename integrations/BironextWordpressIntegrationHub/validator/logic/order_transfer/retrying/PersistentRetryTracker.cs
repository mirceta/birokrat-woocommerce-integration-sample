using core.tools.wooops;
using Newtonsoft.Json;
using si.birokrat.next.common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace validator.logic.order_transfer {
    public class PersistentRetryTracker {

        Dictionary<string, int> retryCounter;
        string filepath;

        public PersistentRetryTracker(string filepath) {
            this.filepath = filepath;

            if (File.Exists(filepath)) {
                string some = File.ReadAllText(filepath);
                retryCounter = JsonConvert.DeserializeObject<Dictionary<string, int>>(some);
            } else {
                retryCounter = new DefaultDictionary<string, int>(() => 0);
            }
        }

        public void AddRetry(string integration_name, OrderTransfer ot) {
            string sig = signature(integration_name, ot);
            if (retryCounter.ContainsKey(sig))
                retryCounter[sig]++;
            else
                retryCounter[sig] = 1;
            persist();
        }
        
        public int Retries(string integration_name, OrderTransfer ot) {
            string sig = signature(integration_name, ot);
            if (retryCounter.ContainsKey(sig)) {
                return retryCounter[sig];
            } else {
                return 0;
            }   
        }

        public void ResetRetries(string integration_name, OrderTransfer ot) {
            string sig = signature(integration_name, ot);
            retryCounter[sig] = 0;
            persist();
        }

        private string signature(string integration_name, OrderTransfer ot) {
            return integration_name + ":" + ot.OrderId + ":" + ot.OrderStatus;
        }

        private void persist() {
            File.WriteAllText(filepath, JsonConvert.SerializeObject(retryCounter));
        }

    }
}
