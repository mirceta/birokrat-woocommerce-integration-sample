using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using common_birowoo;
using core.customers;
using core.structs;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using tests.tools;
using tests_fixture;
using validator.logic;

namespace tests.tests.estrada
{

    public class OrderTestsFactory
    {

        bool fetchOrders;
        DateTime defaultSince;
        
        LazyIntegration lazyIntegration;

        int sessionId;
        TestEnvironmentParams testEnv;

        List<string> skipOrdersWithOutcome;
        int maxOrders;

        SimpleDecoratingFactory<IIntegration, IOutcomeHandler> testOutcomeHandlerDecorator;
        string dataFolder;

        List<string> skipOrdersWithId;

        public OrderTestsFactory(string dataFolder,
            bool fetchOrders, 
            DateTime defaultEarliestOrder, // in the absence of per-integration set earliest order, this setting will be used
            TestEnvironmentParams testenv, 
            LazyIntegration integration,
            List<string> skipOrdersWithOutcome,
            int maxOrders,
            List<string> skipOrdersWithId,
            SimpleDecoratingFactory<IIntegration, IOutcomeHandler> testOutcomeHandlerDecorator) {
            

            this.dataFolder = dataFolder;
            this.fetchOrders = fetchOrders;
            this.defaultSince = defaultEarliestOrder; // this is overriden by any specific configuration in the per-integration testing config
            this.testEnv = testenv;
            this.lazyIntegration = integration;
            this.skipOrdersWithOutcome = skipOrdersWithOutcome;
            this.skipOrdersWithId = skipOrdersWithId;
            this.sessionId = new Random().Next(0, 100);
            this.maxOrders = maxOrders;
            this.testOutcomeHandlerDecorator = testOutcomeHandlerDecorator;
        }

        IIntegration integrationRef = null;

        public async Task<OrderTests> Create(TestingConfigurationWooToBiro config, bool checkBirokratNastavitveValid, IMyLogger logger) {

            var integrationFunc = lazyIntegration.BuildIntegrationAsync;
            integrationRef = await integrationFunc.Invoke();

            IOrderActStage stages = new RetryingOrderProcessor(
                    GetOnFailureProcedure(config.CompanyName), logger);

            var progKeepFactory = GetProgKeeperFactory(config.TestResultsPath, testEnv.Resetprogress);

            return new OrderTests(
                lazyIntegration,
                progKeepFactory,
                GetArranger(config, checkBirokratNastavitveValid, progKeepFactory),
                stages,
                new OrderAsserter(
                    new WooOrderToBiroDocumentComparator(false, new SkipCompare() {
                        Country = false
                    })
                ),
                testOutcomeHandlerDecorator
            );
        }
        IOrderArranger GetArranger(TestingConfigurationWooToBiro config,
            bool checkBirokratNastavitveValid,
            IProgressKeeperFactory progressKeeperFactory) {

            var failureProcedure = GetOnFailureProcedure(config.CompanyName);
            IOrderStore store = BuildStore(config, integrationRef.WooClient);
            return new FilterProcessedOrders(progressKeeperFactory, 
                        new ModifyOrderForTestingArranger(sessionId,
                            new OrderStatusModifier(integrationRef.TestingConfiguration.WooToBiro.TestedOrderStatusSequence,
                                new OrderArranger(failureProcedure,
                                                store,
                                                checkBirokratNastavitveValid: checkBirokratNastavitveValid))));
        }

        private IOrderStore BuildStore(TestingConfigurationWooToBiro config, IOutApiClient wooclient) {
            IOrderStore store = new FolderOrderStore(Path.Combine(dataFolder, "tests_fixture",
                                                "jsons",
                                               "orders",
                                               config.TestResultsPath)
                                            );
            if (fetchOrders) {
                store = new CachedOrderStore(wooclient,
                                new FixDecimalsInOrder(wooclient),
                                (int)DateTime.Now.Subtract(defaultSince).TotalDays,
                                maxOrders,
                                (FolderOrderStore)store);
            }

            DateTime minDate = config.StartDate == DateTime.MinValue ? defaultSince : config.StartDate;
            DateTime maxDate = config.EndDate == DateTime.MaxValue ? DateTime.Today : config.EndDate;
            store = new FilterBetweenDates(minDate, maxDate, store);
            store = new FilterById(skipOrdersWithId, store);

            return new ReturnNewestN(maxOrders, store);
        }

        private List<ISetupStage> GetOnFailureProcedure(string companyName) {
            var tmp = new List<ISetupStage>();
            if (testEnv.Pinger != null)
            {
                tmp.Add(new BironextResetStage(new WaitUntilBironextOnlineForSure(testEnv.Pinger.Deployment, testEnv.Pinger.PingerDelaySeconds)));
            }


            // if (testEnv.Resetdb) tmp.Add(new DatabaseResetStage(testEnv.DatabaseResetter, $"{companyName}_settings.ps1", testEnv.Localsql, testEnv.Localbackuppath));
            // NO WE DO NOT RESET DATABASE DURING TESTS.. DATABASE RESETS HAPPEN ONLY AT THE VERY START OF TESTS!
            return tmp;
        }
        
        private OrderProgressKeeperFactory GetProgKeeperFactory(string path, bool resetprogress) {
            return new OrderProgressKeeperFactory(path, resetprogress, new AlreadyProcessedFilter() {
                MessageContains = skipOrdersWithOutcome
            });
        }
    }
}
