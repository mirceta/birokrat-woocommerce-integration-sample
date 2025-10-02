using core.customers;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;
using si.birokrat.next.common.build;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using tests_webshop.products;

namespace biro_to_woo_playground
{
    partial class Program
    {

        static string program_data_path = Path.Combine(Build.ProjectPath, @"appdata");

        static async Task Main(string[] args)
        {

            string bironextAddress = "https://next.birokrat.si/api/";//"https://next.birokrat.si/api/";
            string integration_data_path = Path.Combine(Build.ProjectPath, @"appdata");
            string python_path = null;
            var coll = new PredefinedIntegrationFactory(false, bironextAddress, integration_data_path, python_path);
            var lazyIntegration = await coll.GetLazyByName("MENHART_BIROTOWOO_STAGING"); // KOLOSET_BIROTOWOO_ONELOOPPROD

            Log.Logger = new LoggerConfiguration()
                            .WriteTo
                            .Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                                theme: AnsiConsoleTheme.Literate)
                            .CreateLogger();


            var microsoftLogger = new SerilogLoggerFactory(Log.Logger)
            .CreateLogger("root");

            /*
            var factory = new BiroToWooExecutorFactory(new ConsoleMyLogger());
            var exec = factory.SingleIterationTesting(integration);
            await exec.Execute(integration);
            */

            var integration = await lazyIntegration.BuildIntegrationAsync.Invoke();
            var ws = new WebshopProductTransferAccessor(integration.WooClient);
            var neki = await ws.List();

            neki = neki.Take(Math.Min(100, neki.Count)).ToList(); // guard against taking 1000 of them
            foreach (var x in neki) {
                if (DateTime.Now.Subtract(DateTime.ParseExact(x.last_event_datetime, "yyyy-MM-ddHH:mm:ss", CultureInfo.InvariantCulture)).TotalHours > 24) {
                    //await integration.WooClient.DeleteProductTransfer(x.product_id);
                }
            }
        }
    }
}
