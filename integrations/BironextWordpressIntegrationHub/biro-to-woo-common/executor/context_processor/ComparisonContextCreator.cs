using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.structs;
using core.tools.zalogaretriever;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.executor.context_processor
{

    public interface IComparisonContextCreator
    {
        Task<BiroOutComparisonContext> Create(IIntegration integration, CancellationToken token);
    }

    public class CachedComparisonContextCreator : IComparisonContextCreator
    {

        IComparisonContextCreator next;
        public CachedComparisonContextCreator(IComparisonContextCreator next) {
            this.next = next;
        }


        BiroOutComparisonContext cached = null;
        public async Task<BiroOutComparisonContext> Create(IIntegration integration, CancellationToken token)
        {
            if (cached == null)
            {
                var tmp = await next.Create(integration, token);
                cached = tmp;
            }
            return cached;
        }
    }

    public class SimpleComparisonContextCreator : IComparisonContextCreator
    {
        public SimpleComparisonContextCreator() { }

        public async Task<BiroOutComparisonContext> Create(IIntegration integration, CancellationToken token)
        {
            var biroArtikelRetriever = (BirokratArtikelRetriever)integration.BiroToWoo.GetBirokratArtikelRetriever();

            List<Dictionary<string, object>> biroItems;
            var addAttrs = integration.BiroToWoo.GetVariationAttributes();
            var queryTerms = addAttrs.ToList().ToDictionary(x => x.Key, x => (object)true);
            biroItems = await biroArtikelRetriever.Query(queryTerms, null);

            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            List<Dictionary<string, object>> wooitems;
            wooitems = await integration.WooClient.GetProducts();

            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            biroItems = FilterInternetNe(biroItems);

            BiroOutComparisonContext cmp = new BiroOutComparisonContext() { biroItems = biroItems, outItems = wooitems };
            return cmp;
        }

        private static List<Dictionary<string, object>> FilterInternetNe(List<Dictionary<string, object>> biroItems)
        {
            string internetFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.Internet);
            biroItems = biroItems.Where(x =>
            {
                return (string)x[internetFieldName] == "-1";
            }).ToList();
            return biroItems;
        }
    }

    public class TestComparisonContextCreator : IComparisonContextCreator
    {

        List<string> sifras;
        public TestComparisonContextCreator(List<string> sifras)
        {
            this.sifras = sifras;
        }

        public async Task<BiroOutComparisonContext> Create(IIntegration integration, CancellationToken token)
        {

            var biroArtikelRetriever = integration.BiroToWoo.GetBirokratArtikelRetriever();

            List<Dictionary<string, object>> biroItems;
            var addAttrs = integration.BiroToWoo.GetVariationAttributes();
            var queryTerms = addAttrs.ToList().ToDictionary(x => x.Key, x => (object)true);
            var artikli = await biroArtikelRetriever.Query(queryTerms, null);

            artikli = artikli.Where(x => sifras.Contains(((string)x[BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla)]))).ToList();

            var context = new BiroOutComparisonContext();
            context.biroItems = artikli;

            context.outItems = await integration.WooClient.GetProducts();

            return context;
        }
    }

    public class PersistedComparisonContextCreator : IComparisonContextCreator
    {

        SimpleComparisonContextCreator creator;
        string cache_filepath;
        public PersistedComparisonContextCreator(SimpleComparisonContextCreator creator, string cache_filepath)
        {
            this.creator = creator;
            this.cache_filepath = cache_filepath;
        }

        public async Task<BiroOutComparisonContext> Create(IIntegration integration, CancellationToken token)
        {

            string path = PrefixFileName(cache_filepath, integration.Name);
            BiroOutComparisonContext result = null;
            if (File.Exists(path))
            {
                Console.WriteLine("Context loaded from disk");
                result = JsonConvert.DeserializeObject<BiroOutComparisonContext>(File.ReadAllText(path));
            }
            else
            {
                Console.WriteLine("Context downloaded online");
                result = await creator.Create(integration, token);
                File.WriteAllText(path, JsonConvert.SerializeObject(result));
            }
            return result;
        }

        private string PrefixFileName(string path, string prefix)
        {
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            string newFileName = prefix + fileName;

            // If the directory part is not empty, combine directory and new file name.
            // Otherwise, just return the new file name.
            string newPath = !string.IsNullOrEmpty(directory) ? Path.Combine(directory, newFileName) : newFileName;

            return newPath;
        }
    }
}
