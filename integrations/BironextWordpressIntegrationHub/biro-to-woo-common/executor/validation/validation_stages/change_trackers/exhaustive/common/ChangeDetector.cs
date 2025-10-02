using core.logic.common_birokrat;
using si.birokrat.next.common.logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace biro_to_woo_common.executor.validation_stages.change_trackers.exhaustive.common
{
    public class ChangeDetector
    {
        private string sifraFieldName;
        private string skuFieldName;
        private bool verbose;
        private bool addproducts_notonwebshop;
        private ProductComparer comparer;

        IMyLogger logger;

        public ChangeDetector(IMyLogger logger, string sifraFieldName, string skuToBirokrat, bool verbose, bool addproducts_notonwebshop)
        {
            this.sifraFieldName = sifraFieldName;
            this.skuFieldName = skuToBirokrat;
            this.verbose = verbose;
            this.addproducts_notonwebshop = addproducts_notonwebshop;
            comparer = new ProductComparer(verbose, logger);
            this.logger = logger;
        }

        public HashSet<string> DetectChanges(List<Dictionary<string, object>> products, List<Dictionary<string, object>> artikli, CancellationToken token)
        {
            HashSet<string> sifrasDiff = new HashSet<string>();
            foreach (var artikel in artikli)
            {
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();
                if (!artikel.ContainsKey(sifraFieldName)) continue;
                DetectChangesSingle(products, sifrasDiff, artikel);
            }

            return sifrasDiff;
        }

        private void DetectChangesSingle(List<Dictionary<string, object>> products, HashSet<string> sifrasDiff, Dictionary<string, object> artikel)
        {
            List<Dictionary<string, object>> matchedProducts = matchArticleToProducts(products, artikel);

            // drafts should get matched, but not change detected because
            // if they do not get matched, then integration will register as if the article is
            // not uploaded and will attempt to upload it which is utterly incorrect.
            if (matchedProducts.Count != 0 && matchedProducts.All(x => (string)x["status"] == "draft"))
                return;

            if (!matchedProducts.Any() && addproducts_notonwebshop)
            {
                sifrasDiff.Add(((string)artikel[sifraFieldName]).Trim());
            }
            else
            {
                foreach (var matchedProduct in matchedProducts)
                {
                    if (comparer.AddOnPriceChange(sifrasDiff, artikel, matchedProduct) ||
                        comparer.AddOnZalogaChange(sifrasDiff, artikel, matchedProduct))
                    {
                        sifrasDiff.Add(((string)artikel[sifraFieldName]).Trim());
                        break;
                    }
                }
            }
        }

        private List<Dictionary<string, object>> matchArticleToProducts(List<Dictionary<string, object>> products, Dictionary<string, object> artikel)
        {
            return products.Where(product =>
                product.ContainsKey("sku") &&
                !string.IsNullOrEmpty((string)product["sku"]) &&
                (string)artikel[skuFieldName] == (string)product["sku"]).ToList();
        }
    }
}