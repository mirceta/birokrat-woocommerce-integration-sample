using BirokratNext.api_clientv2;
using BironextWordpressIntegrationHub.structs;
using birowoo_exceptions;
using BiroWooHub.logic.integration;
using common_abstractions_std.wrappers;
using core.customers.zgeneric;
using core.logic.common_exceptions;
using Newtonsoft.Json;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using tests.tools;
using tests.tools.fixture_setup;

namespace tests.tests.estrada
{
    public class RetryingOrderProcessor : IOrderActStage
    {
        
        List<ISetupStage> onUnhandledException;
        IMyLogger logger;
        public RetryingOrderProcessor(
            List<ISetupStage> onUnhandledException,
            IMyLogger logger) {
            this.onUnhandledException = onUnhandledException;
            this.logger = logger;
        }

        public async Task<Dictionary<string, object>> Act(IIntegration integ, WoocommerceOrder order) {
            for (int i = 0; i < 3; i++) {
                try {
                    try {
                        return await integ.WooToBiro.OnOrderStatusChanged(JsonConvert.SerializeObject(order));
                    } catch (OrderFlowOperationNotFoundException ex) {
                        throw ex;
                    } catch (IntegrationProcessingException ex) {
                        throw ex;
                    } catch (Exception ex) {
                        logger.LogWarning(ex.Message + ex.StackTrace.ToString());
                        throw ex;
                    }
                    break;
                } catch (OrderFlowOperationNotFoundException ex) {
                    throw new IntegrationProcessingException("No order event set for this order.");
                } catch (BironextApiCallException ex) {
                    throw ex;
                } catch (IntegrationProcessingException ex) {
                    throw ex;
                } catch (Exception ex) {
                    logger.LogWarning("Order processing failed... now attempting to resolve.");
                    if (onUnhandledException != null && onUnhandledException.Count != 0) {
                        foreach (var stage in onUnhandledException) {
                            await stage.Work();
                        }
                    }
                    if (i == 2)
                        throw ex;
                    logger.LogWarning("Resolving completed... now attempting to retry");
                }
            }
            return null;
        }
    }
}
