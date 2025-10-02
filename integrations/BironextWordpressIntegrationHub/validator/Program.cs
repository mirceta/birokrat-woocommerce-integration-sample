using allintegrations;
using apirest;
using biro_to_woo.loop;
using BiroWooHub.logic.integration;
using core.customers;
using Newtonsoft.Json;
using si.birokrat.next.common.build;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using validator.logic.order_transfer.accessor;

namespace validator {
    class Program {
        static async Task Main(string[] args) {

            string bironextAddress = "https://next.birokrat.si/api/";
            string integrationdatapath = Path.Combine(Build.SolutionPath, "appdata");

            IIntegrationFactory factory = new PredefinedIntegrationFactory(false, bironextAddress, integrationdatapath);

            factory = await EnvironmentDependentIntegrationFactory.WooToBiroProduction(factory);
            var lazyIntegrations = await factory.GetAllLazy();
            var integrationTasks = lazyIntegrations.Select(async x => await x.BuildIntegrationAsync()).ToList();
            var integrations = (await Task.WhenAll(integrationTasks)).ToList();


            string connectionString = "";

            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("Connection string for sql is empty!");

            await new EveryXSecondsLoop(300,
                new ValidatorSynchronization(
                    integrations,
                    new OrderTransferProcessorRoot_SimpleGuard(),
                    new transfer_data.system.OrderTransferSystemFactory(connectionString),
                    new ConsoleMyLogger()

                )
            ).Execute();
        }
    }
}
