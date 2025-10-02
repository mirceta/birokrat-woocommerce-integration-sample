using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace infrastructure_pinger.chainofresponsibility.eventhandlers
{
    public class ShellyRestartPingerEventHandler : IPingerEventHandler
    {
        IPingerEventHandler next;
        public ShellyRestartPingerEventHandler(IPingerEventHandler next)
        {
            this.next = next;
        }

        public async Task onLongHeartbeat(List<Deployment> allDeployments)
        {
            await next.onLongHeartbeat(allDeployments);
        }

        public async Task onServiceFailure(List<Deployment> fails)
        {
            if (fails.Any(x => x.Name == "BironextProduction"))
            {
                Console.WriteLine("BIRONEXT PRODUCTION FAULT DETECTED! RESETING SHELLY!");
                HttpClient client = new HttpClient() { BaseAddress = new Uri("http://192.168.0.198:3500") };
                string some = client.GetAsync("/api/shelly/setrelayoff/shellyplug-s-9DD12B/0").GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Console.WriteLine("TURNED OFF SHELLY!");
                Thread.Sleep(5000);
                some = client.GetAsync("/api/shelly/setrelayon/shellyplug-s-9DD12B/0").GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Console.WriteLine("TURNED ON SHELLY!");
            }
            if (fails.Any(x => x.Name == "BironextStaging"))
            {

                Console.WriteLine("BIRONEXT STAGING FAULT DETECTED! RESETING SHELLY!");
                HttpClient client = new HttpClient() { BaseAddress = new Uri("http://192.168.0.198:3500") };
                string chome = client.GetAsync("/api/shelly/setrelayoff/shellyplug-s-976F8D/0").GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Console.WriteLine("TURNED OFF SHELLY!");
                Thread.Sleep(5000);
                chome = client.GetAsync("/api/shelly/setrelayon/shellyplug-s-976F8D/0").GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Console.WriteLine("TURNED ON SHELLY!");
            }
            await next.onServiceFailure(fails);
        }

        public async Task OnServiceRestore(List<Deployment> detectedDeployments)
        {
            await next.OnServiceRestore(detectedDeployments);
        }

        public async Task onWarning(List<Deployment> potentiallyFailed)
        {
            await next.onWarning(potentiallyFailed);
        }
    }
}
