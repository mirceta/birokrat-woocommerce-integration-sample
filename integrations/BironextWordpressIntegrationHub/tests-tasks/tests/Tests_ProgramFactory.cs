using BiroWooHub.logic.integration;
using common_birowoo;
using core.customers;
using Newtonsoft.Json;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tests.tests.estrada;
using tests.tests.hisavizij;
using tests_webshop.products;

namespace tests.composition.final_composers.tests
{


    public interface IActualWork
    {
        Task Tests(SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator,
                SimpleDecoratingFactory<IIntegration, IOutcomeHandler> orderDecoratorFactory,
                LazyIntegration integ,
                TestEnvironmentParams testenv,
                IMyLogger logger,
                CancellationToken cancellationToken);

        object AdditionalParams { get; set; }

        string Result { get; }
    }


    public interface IActualWorkFactory
    {
        IActualWork Create();
    }
    public class OrderAndProductTestsFactory : IActualWorkFactory
    {
        public IActualWork Create()
        {
            return new OrderAndProductTests();
        }
    }

    public class OrderAndProductTests : IActualWork
    {
        public object AdditionalParams { get; set; }

        private ITests<string> tests;
        public string Result
        {
            get
            {
                if (tests == null)
                {
                    return "Tests is null";
                }
                else
                {
                    return tests.GetResult();
                }
            }
        }

        public async Task Tests(SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator,
            SimpleDecoratingFactory<IIntegration, IOutcomeHandler> orderDecoratorFactory,
            LazyIntegration integ,
            TestEnvironmentParams testenv,
            IMyLogger logger,
            CancellationToken cancellationToken)
        {

            logger.LogInformation($"Start executing {nameof(OrderAndProductTests)}");
            if (integ.Type == "WOOTOBIRO")
            {
                //tests = new Neki(integ.Name);
                //await tests.Work(cancellationToken);
                await execOrders(orderDecoratorFactory, integ, testenv, logger, cancellationToken);
            }
            else if (integ.Type == "BIROTOWOO")
            {
                //tests = new Neki(integ.Name);
                //await tests.Work(cancellationToken);
                await execProds(productDecorator, integ, testenv, logger, cancellationToken);
            }
            else
            {
                throw new Exception("Integration type not recognized");
            }
        }

        private async Task execProds(SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator, LazyIntegration integ, TestEnvironmentParams testenv, IMyLogger logger, CancellationToken cancellationToken)
        {
            IProductTestsetRetriever retr = null;
            var evaled = await integ.BuildIntegrationAsync();


            var defaultRetr = new DefaultTestsetRetriever(evaled.BiroToWoo.VariableProductBirokratField, 3);


            if (evaled.TestingConfiguration == null)
            {
                retr = defaultRetr;
            }
            else
            {
                string impl = evaled.TestingConfiguration.BiroToWoo.ConcreteProductTestsetRetrieverImplementation;
                var some = new Dictionary<string, Func<IProductTestsetRetriever>> {
                                { "SPICA", () => new SpicaTestsetRetriever() },
                                { "KOLOSET", () => new KolosetTestsetRetriever() }
                            };
                if (impl != null && some.ContainsKey(impl))
                    retr = some[impl]();
                else
                    retr = defaultRetr;

            }



            // ???? hisa vizij: , "x8568" - last arguemnt of FullProductTests!!!
            //testss = new FullProductTests(logger, retr, integ);

            var testset = new Test2ComparisonContextCreator(retr);


            tests = new FProductTests(logger, testset, integ, null, productDecorator, testenv);
            await tests.Work(cancellationToken);
        }

        private async Task execOrders(SimpleDecoratingFactory<IIntegration, IOutcomeHandler> orderDecoratorFactory, LazyIntegration integ, TestEnvironmentParams testenv, IMyLogger logger, CancellationToken cancellationToken)
        {
            var integration = await integ.BuildIntegrationAsync();

            var tmp = JsonConvert.DeserializeAnonymousType((string)AdditionalParams, new { maxOrders = "", SinceDate = "", OrderIdsToIgnore = "" });
            int maxOrders = int.Parse(tmp.maxOrders);
            var sinceDate = DateTime.ParseExact(tmp.SinceDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            List<string> odsToIgnore;
            if (tmp.OrderIdsToIgnore != null && tmp.OrderIdsToIgnore.Any())
                odsToIgnore = new List<string>(tmp.OrderIdsToIgnore.Split(","));
            else
                odsToIgnore = new List<string>();

            OrderTestsFactory orderTestsFactory = new OrderTestsFactory(
                integration.Datafolder,
                fetchOrders: true,
                defaultEarliestOrder: sinceDate,
                testenv: testenv,
                integration: integ,
                skipOrdersWithOutcome: new List<string>() {
                                            "Successful",
                                            //"ValidationException",
                                            //"IntegrationProcessingException",
                                            "OneCentException",
            //"PriceException",
            //"InconsistentWoocommerceOrderPricesException",
            //"Unknown",
            //"CountryException",
            //"ShippingAddressException"
                },
                maxOrders,
                skipOrdersWithId: odsToIgnore,
                orderDecoratorFactory);

            var sm = integration.TestingConfiguration.WooToBiro;

            tests = await orderTestsFactory.Create(sm, checkBirokratNastavitveValid: false, logger);
            await tests.Work(cancellationToken);
        }
    }

    class Neki : ITests<string>
    {
        string integname;
        public Neki(string integname)
        {
            this.integname = integname;
        }
        public string GetResult()
        {
            return integname;
        }

        public async Task Work(CancellationToken token)
        {
        }
    }
}
