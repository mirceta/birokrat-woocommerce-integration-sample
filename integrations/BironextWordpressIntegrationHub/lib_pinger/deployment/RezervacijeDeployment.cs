using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure_pinger.deployment
{
    public class RezervacijeDeployment : Deployment, IPingable {

        public RezervacijeDeployment(string name, Dictionary<string, string> addinfo) : base(name, addinfo) { }

        public override async Task<string> Ping()
        {
            try
            {
                HttpClient client = new HttpClient();
                int yr = DateTime.Now.Year;
                string uri = (AdditionalInfo["serviceaddress"] + $"/api/rezervacije/get?datumOd={yr}-01-01&datumDo={yr}-01-02");
                client.Timeout = new TimeSpan(0, 0, 2);
                string some = await client.GetAsync(uri).Result.Content.ReadAsStringAsync();
                var chome = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(some);
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


    }
}
