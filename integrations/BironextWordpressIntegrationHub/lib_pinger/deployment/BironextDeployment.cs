using BirokratNext;
using BirokratNext.Exceptions;
using infrastructure_pinger.deployment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace infrastructure_pinger
{
    public class BironextDeployment : Deployment, IPingable
    {

        const int TIMEOUT = 20;

        public BironextDeployment(string name, Dictionary<string, string> addinfo) : base(name, addinfo) { }
        public override async Task<string> Ping()
        {
            int retryCount = 0;
            while(true)
            {
                try
                {
                    var apiClient = new ApiClientV2(
                        apiAddress: AdditionalInfo["apiAddress"],
                        apiKey: AdditionalInfo["apiKey"],
                        TIMEOUT);

                    await apiClient.Logout();
                    var parameters = await apiClient.cumulative.Parametri("sifranti/artikli/stanjezaloge");
                    if (parameters.Count < 1)
                    {
                        return JsonConvert.SerializeObject(parameters);
                    }

                    return null;
                }
                catch (ConcurrentRequestsNotAllowedException ex)
                {
                    retryCount++;
                    if (retryCount > 5)
                        return ex.ToString();
                    await Task.Delay(5000);
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
        }
    }
}
