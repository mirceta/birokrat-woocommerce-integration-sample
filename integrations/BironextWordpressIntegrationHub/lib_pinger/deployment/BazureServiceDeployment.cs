using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using infrastructure_pinger.deployment;

namespace infrastructure_pinger
{
    public class BazureServiceDeployment : Deployment, IPingable
    {
        public BazureServiceDeployment(string name, Dictionary<string, string> addinfo) : base(name, addinfo) { }
        public override async Task<string> Ping()
        {
            for (int i = 0; i < 10; i++) // retry a couple of times because the service gets restarted every hour and could be it got pinged during restart 
            {
                try
                {
                    HttpClient client = new HttpClient();
                    string uri = (AdditionalInfo["bironextAddress"] + "/test/is-bazure-service-on").Replace("//", "/").Replace("http:/", "http://").Replace("https:/", "https://").Replace("///", "//");
                    string some = await client.GetAsync(uri).Result.Content.ReadAsStringAsync();
                    bool success = bool.Parse(some);
                    if (!success)
                    {
                        Thread.Sleep(5000);
                        continue;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                }
            }
            return "Failed";
        }
    }
}
