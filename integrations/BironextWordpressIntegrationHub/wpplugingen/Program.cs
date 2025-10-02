using BiroWooHub.logic.integration;
using core.customers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace wpplugingen {
    class Program
    {
        static async Task Main(string[] args)
        {
            IIntegration integ = await getIntegrationFromIntegrationFactory();

            string deploymentpath = @"C:\Users\birowoo-oldest-dev\desktop\woodeployment";

            await new WppluginGen(deploymentpath).createPlugin(integ.PhpPluginConfigVal, integ.BiroClient.ApiKey);
        }

        private static async Task<IIntegration> getIntegrationFromIntegrationFactory()
        {
            string integrationName = "OSNOVNA_WOOTOBIRO_KONE";
            string bironextaddress = "https://next.birokrat.si/api/";
            var factory = new PredefinedIntegrationFactory(false, bironextaddress, "");
            var allIntegrations = await factory.GetAllLazy();
            var integrationTask = allIntegrations.Where(x => x.Name == integrationName).Single().BuildIntegrationAsync();
            IIntegration integ = await integrationTask;
            return integ;
        }
    }
}
