using biro_to_woo.logic.change_trackers.exhaustive;
using biro_to_woo.loop;
using biro_to_woo_common;
using biro_to_woo_common.error_handling.errors;
using biro_to_woo_common.executor;
using biro_to_woo_common.executor.context_processor;
using biro_to_woo_common.executor.detection_actions;
using biro_to_woo_common.executor.validation;
using biro_to_woo_common.executor.validation_stages.change_trackers.exhaustive;
using biro_to_woo_common.executor.validation_stages;
using BirokratNext.api_clientv2;
using birowoo_exceptions;
using BiroWoocommerceHub.flows;
using BiroWooHub.logic.integration;
using core.customers;
using core.logic.common_birokrat;
using core.logic.mapping_biro_to_woo;
using core.tools.wooops;
using core.tools.zalogaretriever;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;
using si.birokrat.next.common.build;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using tests;
using tests_webshop.products;
using biro_to_woo_common.executor.validation.validation_stages.validators;
using si.birokrat.next.common.logging;
using common_birowoo;
using core.error_handling.handlers;
using transfer_data.products;

namespace biro_to_woo_single
{
    partial class Program
    {

        static string program_data_path = Path.Combine(Build.ProjectPath, @"appdata");

        static async Task Main(string[] args) {
            Console.WriteLine("Hello World!");


            string bironextAddress = "https://next.birokrat.si/api/";//"https://next.birokrat.si/api/";
            string integration_data_path = Path.Combine(Build.ProjectPath, @"appdata");
            string python_path = null;
            var coll = new PredefinedIntegrationFactory(false, bironextAddress, integration_data_path, python_path);
            var integration = await coll.GetLazyByName("SPICA_BIROTOWOO_PRODUCTION"); // KOLOSET_BIROTOWOO_ONELOOPPROD

            Log.Logger = new LoggerConfiguration()
                            .WriteTo
                            .Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                                theme: AnsiConsoleTheme.Literate)
                            .CreateLogger();

            
            var microsoftLogger = new SerilogLoggerFactory(Log.Logger)
            .CreateLogger("root");



            //var validator = new BiroToWooValidator(microsoftLogger);
            //await validator.Validate(integration);

            /*
            var factory = new BiroToWooExecutorFactory(microsoftLogger);

            var exec = factory.SingleIterationTesting(integration);
            await exec.Execute(integration);
            */

            /*
            var pregled = new PodrobniPregledArtiklov();
            pregled.ModifyParameters(new Dictionary<string, object>() {
                { "Artikel", "*703743*" }
            });
            var tmp = await pregled.GetPodrobniPregledArtiklov(integration.BiroClient);

            tmp = tmp.Where(x =>
            {
                return !string.IsNullOrEmpty((string)x["Prenesi v e-shop"]);
            }).ToList();





            var sifre = tmp.Select(x => (string)x["Artikel"]).ToList();
            */

            var sifre = new List<string> { "TDDŠM38" };

            var result = await integration.BuildIntegrationAsync.Invoke();
            await new ArticleChangeUploader(result, (x) => new ReportingBiroToWoo(new ConsoleErrorHandler(), result.BiroToWoo)).NotifyChanges(sifre, new System.Threading.CancellationToken());

            var exec = new BiroToWooExecutor(new ConsoleMyLogger(),
                   new TestComparisonContextCreator(sifre),
                   new List<IBiroToOutValidationStage>()
                    {
                        new DatabaseAgreementComplianceVerifier().Get(result),
                        new ExhaustiveArtikelChangeTrackerFactory(new ConsoleMyLogger(), 10).Create(result)
                    },
                   new WebshopErrorHandler(new ConsolePrintProductTransferAccessor()),
                   new ConsoleDetectionAction());

            await exec.Execute(result, new System.Threading.CancellationToken());
        }

        private static async Task WebshopReuploadVarProds(IIntegration integration) {
            
            // delete all
            var deletor = new WebshopDeleteVarProds_ThenReturnSifras();

            await deletor.DeleteVariableProductsFromWoocommerce(integration);

            var chome = await new BirokratGetVarProds(integration).GetVariableProductsFromBirokrat();
            await deletor.DeleteProductsRetryingVySku(integration, chome);


            // reupload
            foreach (string c in chome) {
                await integration.BiroToWoo.OnArticleChanged(c);
            }

            chome = chome.Take(5).ToList();

            // see if on change works as well
            foreach (string c in chome) {
                await UpdatePrice(integration, c, 0.01);
            }

            foreach (string c in chome) {
                await integration.BiroToWoo.OnArticleChanged(c);
            }


            // change back to origi price
            foreach (string c in chome) {
                await UpdatePrice(integration, c, -0.01);
            }

            foreach (string c in chome) {
                await integration.BiroToWoo.OnArticleChanged(c);
            }
        }

        private static async Task UpdatePrice(IIntegration integration, string c, double change) {
            var x = await GWooOps.GetWooProductBySku(integration.WooClient, c);

            string strregpricewtax = GWooOps.SerializeDblWooProperty(x["regular_price"]);
            if (GWooOps.SerializeDblWooProperty(x["regular_price"]) == "0") {
                strregpricewtax = GWooOps.SerializeDblWooProperty(x["price"]);
            }
            double some = Tools.ParseDoubleBigBrainTime(strregpricewtax);
            UpdatePrice(integration, @"sifranti\artikli\prodajniartikli-storitve", c, some + change);
        }

        private static void UpdatePrice(IIntegration integ, string sifrantRoute, string sifra, double price) {
            var parame = integ.BiroClient.sifrant.UpdateParameters(sifrantRoute, sifra).GetAwaiter().GetResult();
            var dict = parame
            .GroupBy(x => x.Koda)
            .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            dict["PCsPD"] = string.Format("{0:0,00}", price + "");
            var result1 = integ.BiroClient.sifrant.Update(sifrantRoute, dict).GetAwaiter().GetResult();
        }
    }
}
