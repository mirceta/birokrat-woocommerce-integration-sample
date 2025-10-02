using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using infrastructure_pinger.deployment;

namespace infrastructure_pinger
{
    public class BiroWooDeployment : Deployment, IPingable
    {
        public BiroWooDeployment(string name, Dictionary<string, string> addinfo) : base(name, addinfo) { }
        public override async Task<string> Ping()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 2);
                string some = await client.GetAsync($"{AdditionalInfo["birowooaddress"]}/main/secs-since-last-loop").Result.Content.ReadAsStringAsync();
                if (int.Parse(some) > 3600)
                {
                    return "It has been more than 3600 seconds since last loop completed!";
                }

                string some1 = await client.GetAsync($"{AdditionalInfo["birowooaddress"]}/main/ping").Result.Content.ReadAsStringAsync();
                if (some1 != "Pong")
                {
                    return some1;
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
