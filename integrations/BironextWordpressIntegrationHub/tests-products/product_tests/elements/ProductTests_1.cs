using biro_to_woo_common.executor;
using biro_to_woo_common.executor.context_processor;
using biro_to_woo_common.executor.detection_actions;
using birowoo_exceptions;
using BiroWoocommerceHub.structs_wc_to_biro;
using BiroWooHub.logic.integration;
using common_birowoo;
using core.customers;
using core.structs;
using core.tools.zalogaretriever;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tests.tests.estrada;
using tests_bironext_pinger;
using tests_webshop.products;
using transfer_data.products;

namespace tests.tests.hisavizij
{
    public class FProductTests : ITests<string> {

        IIntegration integration;
        LazyIntegration lazyIntegration;
        IComparisonContextCreator testset;

        IZalogaRetriever zaloga;
        string sestavljenArtikel = null;

        IMyLogger logger;
        List<TestEqualAddition> additionalTests;
        SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> decoratingFactory;
        LocalMode_BironextPinger pinger;

        public FProductTests(
            IMyLogger logger,
            IComparisonContextCreator testset,
            LazyIntegration lazyIntegration,
            List<TestEqualAddition> additionalTests,
            SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> decoratingFactory,
            TestEnvironmentParams testenv,
            string sestavljenArtikel = null) {
            this.logger = logger;
            this.lazyIntegration = lazyIntegration;
            this.sestavljenArtikel = sestavljenArtikel;
            this.testset = testset;
            this.additionalTests = additionalTests;
            this.decoratingFactory = decoratingFactory;
            this.pinger = testenv.Pinger;
        }
        public async Task Work(CancellationToken token)
        {
            // creation phase
            integration = await lazyIntegration.BuildIntegrationAsync.Invoke();

            if (!(integration.WooClient.Address.Contains("svece") || integration.WooClient.Address.Contains("konecreative"))) {
                throw new Exception("DANGER! Production out clients are not allowed in this class!");
            }
            zaloga = integration.ValidationComponents.Zaloga;
            await oldTestsNewWay(token);            
            // now sestavljeni artikli tests!
            // - to dejansko ni problem - dovolj je test v HisaVizijSestavljeniProduktiTests
            // zakaj? Zato ker ni del BiroToWooExecutorja - to je v bistvu del ZalogaRetrieverja, ki je testiran posebej od tega.
        }

        private async Task oldTestsNewWay(CancellationToken token)
        {

            bool addprods = !integration.Options.Birotowoo_changetracker_dontincludearticlesthatarenotonwebshop;
            if (!addprods)
            {
                string err = "These tests include uploading articles and the current integration has disabled\n";
                err += "artikel uploading therefore the tests will not work. Cannot run tests on this integration.";
                throw new Exception(err);
            }

            IProductTransferAccessor baseAccessor = new ConsolePrintProductTransferAccessor();
            if (decoratingFactory != null)
            {
                baseAccessor = decoratingFactory.Decorate(integration, baseAccessor);
            }
            var erh = new TestErrorHandler(new WebshopErrorHandler(baseAccessor));
            BiroToWooExecutor executor = new BiroToWooExecutorFactory(logger).Create(integration,
                context: testset,
                handler: erh,
                detectionAction: new ArticleChangeUploader(integration, (integ) =>
                {
                    return new ReportingBiroToWoo(erh, integ.BiroToWoo);
                }),
                verify: false,
                detectChanges: true);
            await Work(erh, executor, token);
        }

        private async Task Work(TestErrorHandler erh, BiroToWooExecutor executor, CancellationToken token)
        {
            await new ProductTestsWooDeleter(integration)
                        .FullDeleteProductsWithSkusFromWebshop(
                          Decor(testset), token);

            var prods = await integration.WooClient.GetProducts();
            int afterDeleteNumProds = prods.Count;

            var runexec = Decor(new RunExecutorThenVerifyNoErrors(integration, executor, erh, token));

            await runexec.Execute(); ; // upload new articles
            prods = await integration.WooClient.GetProducts();

            // THIS CANNOT WORK BECAUSE YOU CAN BE RUNNING PARALLEL TESTS!
            // ANOTHER PROBLEM: MULTIPLE PRODUCTS MAY GET UPLOADED WHEN YOU UPLOAD VARIABLE PRODUCT
            //Verify_ExpectedNumberOfArticlesHasBeenUploaded(executor, 
            //                                    prods.Count, afterDeleteNumProds);


            var pc = new BiroPriceChanger(integration, results, erh, executor, token, zaloga);
            await Decor(pc).Execute();
            var sifre = pc.Sifre;

            await runexec.Execute();
            var fv = new ProductTestsFinalValidation(integration, logger, zaloga, additionalTests, results, sifre);
            await Decor(fv).Execute();
        }

        private static void Verify_ExpectedNumberOfArticlesHasBeenUploaded(BiroToWooExecutor executor, 
            int afterUploadCount, 
            int beforeUploadCount)
        {
            int testsetCount = executor.LastRunContext().biroItems.Count;
            int expectedCount = beforeUploadCount + testsetCount;
            if (afterUploadCount != expectedCount)
            {
                string t = $"BiroToWooExecutor was supposed to upload {testsetCount} new articles.\n";
                t += $"instead there were only {afterUploadCount - beforeUploadCount} new uploads.\n";
                t += $"The detector has failed to detect discrepancies!";
                throw new ProductTestException(t); // this should be TestException!!!!!
            }
        }

        
        private IDecor Decor(IDecor decor) {
            var fac = new BironextGuardFactory(logger, pinger);
            return new WithGuard(fac, decor);
        }
        private IComparisonContextCreator Decor(IComparisonContextCreator decor) {
            var fac = new BironextGuardFactory(logger, pinger);
            return new WithGuar(fac, decor);
        }

        #region [auxiliary]
        readonly TracedList results = new TracedList();
        object lck = new object();
        List<string> traces = new List<string>();
        public string GetResult() {

            string result = "";
            lock (lck)
            {
                StackTrace stackTrace = new StackTrace(true); // true to capture file info
                traces.Add(stackTrace.ToString());
                result = string.Join(Environment.NewLine, results.Get());

                result += string.Join(Environment.NewLine, results.getTRACES());

                result += string.Join(Environment.NewLine, traces);
            }
            return result;
        }
        #endregion
    }

    public class TracedList {

        private List<string> traces = new List<string>();

        private List<string> lst;
        object lck = new object();
        public TracedList() {
            lst = new List<string>();
        }
        public void Add(string item)
        {
            lock (lck)
            {
                StackTrace stackTrace = new StackTrace(true); // true to capture file info
                traces.Add(stackTrace.ToString());
                lst.Add(item);
            }
        }

        public List<string> Get() {
            return new List<string>(lst);
        }

        public List<string> getTRACES() {
            return traces;
        }

    }

    interface IDecor {
        Task Execute();
    }
    class WithGuar : IComparisonContextCreator
    {
        BironextGuardFactory factory;
        IComparisonContextCreator next;
        public WithGuar(BironextGuardFactory factory, IComparisonContextCreator next)
        {
            this.factory = factory;
            this.next = next;
        }
        public async Task<BiroOutComparisonContext> Create(IIntegration integration, CancellationToken token)
        {
            BiroOutComparisonContext creator = null;
            var guard = factory.Create(async () => { creator = await next.Create(integration, token); });
            await guard.Execute();
            return creator;
        }
    }
    class WithGuard : IDecor
    {
        BironextGuardFactory factory;
        IDecor decor;
        public WithGuard(BironextGuardFactory factory, IDecor decor)
        {
            this.factory= factory;
            this.decor = decor;
        }
        public async Task Execute()
        {
            var guard = factory.Create(async () => await decor.Execute());
            await guard.Execute();
        }
    }

    class BironextGuardFactory {
        IMyLogger logger;
        LocalMode_BironextPinger pinger;
        public BironextGuardFactory(IMyLogger logger, LocalMode_BironextPinger pinger)
        {
            this.logger = logger;
            this.pinger = pinger;
        }

        public BironextGuard Create(Func<Task> action) {
            return new BironextGuard(logger, pinger, action);
        }
    }
    class BironextGuard
    {
        Func<Task> action;
        LocalMode_BironextPinger pinger;
        IMyLogger logger;
        public BironextGuard(IMyLogger logger, LocalMode_BironextPinger pinger, Func<Task> action) {
            this.action = action;
            this.pinger = pinger;
            this.logger = logger;
        }
        public async Task Execute()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await action();
                    break;
                }
                catch (ProductTestException ex) {
                    throw ex;
                }
                catch (Exception ex)
                {
                    logger.LogError($"BironextGuard catch (retrying {i}): " + ex.Message + ex.StackTrace.ToString());
                    var waiter = new WaitUntilBironextOnlineForSure(pinger.Deployment, pinger.PingerDelaySeconds);
                    await waiter.Wait();
                }
            }
        }
    }

    class RunExecutorThenVerifyNoErrors : IDecor {
        
        BiroToWooExecutor executor; 
        TestErrorHandler erh; 
        CancellationToken token;
        IIntegration integ;

        public RunExecutorThenVerifyNoErrors(IIntegration integ, BiroToWooExecutor executor, TestErrorHandler erh, CancellationToken token)
        {
            this.executor = executor;
            this.erh = erh;
            this.token = token;
            this.integ = integ;
        }

        public async Task Execute()
        {
            await executor.Execute(integ, token);
            var reports = erh.GetReports();
            if (reports.Count > 0)
                Debugger.Break(); // we expect there to be no unexpected events!
        }
    }

    class BiroPriceChanger : IDecor
    {

        IIntegration integ;
        TracedList results;
        TestErrorHandler erh;
        BiroToWooExecutor executor; 
        CancellationToken token;
        IZalogaRetriever zaloga;


        List<string> sifre;
        public List<string> Sifre { get => sifre;  }

        public BiroPriceChanger(IIntegration integ, TracedList results, TestErrorHandler erh, BiroToWooExecutor executor, CancellationToken token, IZalogaRetriever zaloga)
        {
            this.integ = integ;
            this.results = results;
            this.erh = erh;
            this.executor = executor;
            this.token = token;
            this.zaloga = zaloga;
        }

        public async Task Execute()
        {
            var biroChanger = new ShouldChangePriceAndChangeZaloga(integ.BiroClient, zaloga);
            var sifre = executor.LastRunContext().biroItems.Select(x => (string)x["txtSifraArtikla"]).ToList();
            foreach (var x in sifre)
            {
                await biroChanger.Execute(x, results);
            }
            this.sifre = sifre;
        }
    }

    class ProductTestsFinalValidation : IDecor
    {

        IIntegration integ;
        IMyLogger logger;
        IZalogaRetriever zaloga;
        List<TestEqualAddition> additionalTests;
        TracedList results;
        List<string> sifre;
        public ProductTestsFinalValidation(IIntegration integ, IMyLogger logger, IZalogaRetriever zaloga,
            List<TestEqualAddition> additionalTests, TracedList results, List<string> sifre) {
            this.integ = integ;
            this.logger = logger;
            this.zaloga = zaloga;
            this.results = results;
            this.sifre = sifre;
        }

        public async Task Execute()
        {
            await integ.WooClient.GetProducts(); // refresh cache
            var ver = new VerifyProduct(logger, integ, zaloga, additionalTests, results);
            try
            {
                
                sifre.ForEach(async x => {
                    results.Add("<bold>");
                    results.Add($"sifra: {x}");
                    await ver.Verify(x);
                    results.Add("</bold>");
                });
            }
            catch (Exception ex)
            {
                results.Add(ex.ToString());
                throw ex;
            }
        }
    }
}
