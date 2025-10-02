using biro_to_woo_common.error_handling.errors;
using BiroWooHub.logic.integration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using tests_webshop.products;
using transfer_data.products;

namespace biro_to_woo_common.executor.detection_actions
{
    public class ArticleChangeUploader : IDetectionAction
    {
        private IIntegration integration;
        Func<IIntegration, ReportingBiroToWoo> decoratorFactory;

        public ArticleChangeUploader(IIntegration integration, 
            Func<IIntegration, ReportingBiroToWoo> decoratorFactory)
        {
            this.integration = integration;
            this.decoratorFactory = decoratorFactory;
        }

        public async Task NotifyChanges(List<string> successfulItemSifras, CancellationToken token)
        {
            var inner = decoratorFactory(this.integration);
            foreach (var sifra in successfulItemSifras)
            {
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
                await inner.OnArticleChanged(sifra);
            }
        }
    }
}


/*
 
        public ArticleChangeUploader(IIntegration integration)
        {
            this.integration = integration;
            this.decoratorFactory = (integ) => {
                // by default we want to post reports to the customer's webshop
                var inner = integ.BiroToWoo;
                WebshopProductTransferAccessor accessor = new WebshopProductTransferAccessor(integration.WooClient);
                var erh = new WebshopErrorHandler(accessor);
                return new ReportingBiroToWoo(erh, inner);
            };
        }
 */