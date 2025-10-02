using BiroWooHub.logic.integration;
using common_birowoo;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using tests.composition.final_composers.tests;
using tests.composition.root_builder;
using tests.interfaces;
using tests.tests.estrada;
using tests_webshop.products;

namespace tests.composition.final_composers
{
    public class WrapWithFactoriesHelper
    {

        public static ITestsFactory Wrap(IActualWorkFactory actualWorkFactory,
            SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator,
            SimpleDecoratingFactory<IIntegration, IOutcomeHandler> orderDecoratorFactory,
            Action onFinish = null
        )
        {
            return new TestFactory((integr, testenv, logger, additionalParams) => {

                var actualWork = actualWorkFactory.Create();
                var tests = new Tests(work: async (cancellationToken) =>
                    {
                        try
                        {
                            actualWork.AdditionalParams = additionalParams;
                            await actualWork.Tests(productDecorator, orderDecoratorFactory, integr, testenv, logger, cancellationToken);
                        }
                        finally
                        {
                            if (onFinish != null)
                                onFinish();
                        }
                    }, 
                    () => actualWork.Result);
                return tests;

            });
        }
    }
}
