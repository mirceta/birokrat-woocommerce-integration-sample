using biro_to_woo_common.error_handling.errors;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.tools.zalogaretriever;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tests_webshop.products;

namespace transfer_data.products
{
    public class ReportingBiroToWoo : IBiroToWoo
    {

        IProductTransferAccessor accessor;
        IBiroToWoo next;
        IErrorHandler handler;

        public ReportingBiroToWoo(IErrorHandler errorHandler, IBiroToWoo next)
        {
            if (errorHandler == null)
                throw new ArgumentNullException("errorHandler");
            if (next == null)
                throw new ArgumentNullException("next");
            this.next = next;

            handler = errorHandler;
        }

        public async Task OnArticleChanged(string sifra)
        {
            Console.WriteLine($"Processing {sifra}!");

            Exception exc = null;
            try
            {
                await next.OnArticleChanged(sifra);
                Console.WriteLine($"{sifra} success!");
            }
            catch (Exception ex)
            {
                exc = ex;
                Console.WriteLine($"{sifra} failed!");

            }
            await handler.HandleError(sifra, exc);
        }

        #region [boilerplate]
        public BirokratField SkuBirokratField { get => next.SkuBirokratField; set => next.SkuBirokratField = value; }
        public BirokratField VariableProductBirokratField { get => next.VariableProductBirokratField; set => next.VariableProductBirokratField = value; }

        public IBirokratArtikelRetriever GetBirokratArtikelRetriever()
        {
            return next.GetBirokratArtikelRetriever();
        }

        public async Task OnArticleAdded(string sifra)
        {
            await next.OnArticleAdded(sifra);
        }
        public async Task OnArticleDeleted(string sifra)
        {
            await next.OnArticleDeleted(sifra);
        }

        public void SetBirokratArtikelRetriever(IBirokratArtikelRetriever zaloga)
        {
            next.SetBirokratArtikelRetriever(zaloga);
        }

        public Dictionary<string, string> GetVariationAttributes()
        {
            return next.GetVariationAttributes();
        }
        #endregion
    }
}
