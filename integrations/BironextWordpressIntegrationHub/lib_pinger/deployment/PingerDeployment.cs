using si.birokrat.next.common.networking;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure_pinger.deployment
{
    public class PingerDeployment : Deployment, IPingable
    {

        const string PINGER_PORT = "5732";
        public PingerDeployment(string name, Dictionary<string, string> addinfo) : base(name, addinfo) { }
        public override async Task<string> Ping()
        {
            // do not ping self
            if (NetworkingUtils.GetLocalIPAddress() == AdditionalInfo["ipaddress"])
                return null;

            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 2);
                string some = await client.GetAsync($"http://{AdditionalInfo["ipaddress"]}:{PINGER_PORT}").Result.Content.ReadAsStringAsync();
                if (some != "Operational")
                {
                    return some;
                }
                return null;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
