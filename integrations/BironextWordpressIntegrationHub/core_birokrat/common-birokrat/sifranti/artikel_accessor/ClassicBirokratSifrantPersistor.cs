using BirokratNext;
using birowoo_exceptions;
using core.logic.common_birokrat;
using core.tools.birokratops;
using core.tools.zalogaretriever;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{

    public class ClassicBirokratSifrantPersistor
    {

        IApiClientV2 client;
        ISifrantSearchStrategy searchStrategy;

        public ClassicBirokratSifrantPersistor(IApiClientV2 client, 
            ICountryMapper mapper = null,
            ISifrantSearchStrategy searchStrategy = null) {
            this.client = client;
            if (searchStrategy == null) {
                this.searchStrategy = new DefaultSearchStrategy(client);
            } else {
                this.searchStrategy = searchStrategy;
            }
        }
         

        public async Task<string> SearchThenIfNotFoundCreate(SearchThenIfNotFoundCreateArgs args, List<string> codesToUpdate = null) {
            var res = await GetRequestedBirokratItem(args);
            if (res == null) {
                await AddSifrantEntry(args);
                return await GetRequestedBirokratItem(args);
            } else if (codesToUpdate != null) {
                await client.sifrant.UpdateParameters(args.sifrantpath, res);
                var pck = args.pack.Where(x => codesToUpdate.Contains(x.Key)).ToDictionary(x => x.Key, y => y.Value);
                await client.sifrant.Update(args.sifrantpath, pck);
            }
            return res;
        }

        public async Task<string> GetRequestedBirokratItem(SearchThenIfNotFoundCreateArgs args) {
            return await searchStrategy.GetRequestedBirokratItem(args);
        }

        public async Task<string> AddSifrantEntry(SearchThenIfNotFoundCreateArgs args) {
            string ret = (string)await client.sifrant.Create(args.sifrantpath, args.pack);
            if (ret.StartsWith("Error 5")) {
                throw new UnableToCreatePartnerServerException(ret);
            }
            if (ret.StartsWith("Error 4")) {
                throw new UnableToCreatePartnerBadRequestException(ret);
            }
            return ret;
        }
    }

    public class SearchThenIfNotFoundCreateArgs {
        public string sifrantpath { get; set; }
        public string searchterm { get; set; } // the term to search to find the record we want to create
        public string nameoffieldtocomparewith { get; set; } // which attribute does wooitemidentifer map to in birokrat?
        public string valuetocomparewith { get; set; } // the value of the woo item identifier
        public string fieldtoreturn { get; set; }
        
        public Dictionary<string, object> pack { get; set; } // packed params
    }


    public interface ISifrantSearchStrategy {
        Task<string> GetRequestedBirokratItem(SearchThenIfNotFoundCreateArgs args);
        
    }
    public class BirokratArtikelRetrieverSearchStrategy : ISifrantSearchStrategy {

        IApiClientV2 client;
        public BirokratArtikelRetrieverSearchStrategy(IApiClientV2 client) {
            this.client = client;
        }

        public async Task<string> GetRequestedBirokratItem(SearchThenIfNotFoundCreateArgs args) {

            var podpreg = new BirokratArtikelRetriever(client, null);
            var dic = await podpreg.Query(null, new Dictionary<string, object>() { { "Artikel", $"*{args.searchterm}*" } });
            foreach (var item in dic) {
                if ((string)item[args.nameoffieldtocomparewith] == args.valuetocomparewith) {
                    return (string)item[args.fieldtoreturn];
                }
            }
            return null;
        }
    }
    class DefaultSearchStrategy : ISifrantSearchStrategy {

        IApiClientV2 client;
        public DefaultSearchStrategy(IApiClientV2 client) {
            this.client = client;
        }
        
        public async Task<string> GetRequestedBirokratItem(SearchThenIfNotFoundCreateArgs args) {
            var dic = await client.sifrant.Podatki(args.sifrantpath, args.searchterm);
            foreach (var item in dic) {
                if ((string)item[args.nameoffieldtocomparewith] == args.valuetocomparewith) {
                    return (string)item[args.fieldtoreturn];
                }
            }
            return null;
        }
    }
}
