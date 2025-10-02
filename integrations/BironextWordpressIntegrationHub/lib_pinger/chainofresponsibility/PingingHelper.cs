using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure_pinger.chainofresponsibility {
    public class PingingHelper {

        public static async Task Pingy(List<Deployment> deployments) {
            foreach (Deployment dep in deployments) {

                try {
                    string result = await dep.Ping();
                    if (string.IsNullOrEmpty(result)) {
                        dep.UnsuccessfulPingsInARow = 0;
                    } else {
                        dep.UnsuccessfulPingsInARow++;
                    }
                    dep.PingResult = result;
                } catch (Exception ex) {
                    throw ex;
                }

            }
        }
    }
}
