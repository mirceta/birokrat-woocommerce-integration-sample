using allintegrations;
using biro_to_woo.logic.change_trackers.exhaustive;
using biro_to_woo.loop;
using biro_to_woo_common;
using biro_to_woo_common.change_trackers.loop.async_per_integration;
using BiroWooHub.logic.integration;
using core.customers;
using core.logic.common_birokrat;
using logging;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Serilog;
using Serilog.Extensions.Logging;
using si.birokrat.next.common.build;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo
{
    class Program
    {
        static string bironextAddress = "";
        static string program_data_path = Path.Combine(Build.ProjectPath, @"appdata");
        static string integration_data_path;

        private static async Task MainLoop() {


            List<IIntegration> ints = null;

            while (true)
            {
                try
                {
                    var coll = new PredefinedIntegrationFactory(false, bironextAddress, integration_data_path, pythonpath);
                    var fac = await EnvironmentDependentIntegrationFactory.BiroToWooProduction(coll);
                    var tmp = (await fac.GetAllLazy());
                    var integrationTasks = tmp.Select(async x => await x.BuildIntegrationAsync()).ToList();
                    ints = (await Task.WhenAll(integrationTasks)).ToList();
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error("Failure while loading integrations (before main loop). Retry loading integrations in 30 seconds " + ex.Message + ex.StackTrace);
                    Thread.Sleep(30000);
                }
            }

            var microsoftLogger = new SerilogLoggerFactory(Log.Logger)
            .CreateLogger("root");

            /*
            var some = new EveryXSecondsLoop(120,
                new SequentialSynchronization(ints,
                new PersistentArtikelUploadQueueFactory(program_data_path),
                new ExhaustiveArtikelChangeTrackerFactory(microsoftLogger, 10)));
            await some.Execute();
            */
                

        }

        #region [boilerplate]
        static string pythonpath = null;

        static async Task Main(string[] args) {
            
            var projectPath = Build.ProjectPath;
            if (args.Length > 0) {
                projectPath = args[0];
                pythonpath = args[0];
                // this sleep needs to happen because when running through task scheduler. If you
                // run the program on system startup, then you may get to the web calls before the
                // network connection is established on the server.
                Thread.Sleep(30000);
            }


            var secretsPath = Path.Combine(projectPath, "appsettings.Secrets.json");
            var secretsExamplePath = Path.Combine(projectPath, "appsettings.Secrets.Example.json");

            if (!File.Exists(secretsPath))
            {
                File.Copy(secretsExamplePath, secretsPath);
                Console.ReadLine();
                Environment.Exit(-1);
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(projectPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Secrets.json", optional: false, reloadOnChange: true)
                .Build();

            bironextAddress = configuration["BironextAddress"];
            if (string.IsNullOrEmpty(bironextAddress))
                throw new Exception("Bironext address is not set. Please fill the configuration!");
            bironextAddress = BironextAddressParser.CorrectAddress(bironextAddress);

            program_data_path = configuration["Datafolder"];
            if (string.IsNullOrEmpty(program_data_path)) {
                throw new Exception("Integration data path cannot be null!");
            }

            integration_data_path = configuration["Integrationdatapath"];
            if (string.IsNullOrEmpty(integration_data_path)) {
                throw new Exception("Integration data path cannot be null!");
            }

            string loggingServer = configuration["LoggingSqlServer"];
            if (string.IsNullOrEmpty(loggingServer))
                throw new Exception("Logging sql server is not set. Please fill the configuration!");


            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            BironextWoocommerceHubLogging.LoggingSetup(loggingServer);

            await MainLoop();
        }
        
        static void OnProcessExit(object sender, EventArgs e) {
            Log.CloseAndFlush();
        }
        #endregion

    }
}
