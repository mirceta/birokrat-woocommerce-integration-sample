using allintegrations.customers.eigrace;
using allintegrations.customers.ekodrive;
using allintegrations.customers.poledancerka;
using allintegrations.customers.spicasport;
using allintegrations_factories.customers.estrada;
using allintegrations_factories.customers.hisavizij;
using allintegrations_factories.customers.nasa_testna;
using allintegrations_factories.customers.NOVE;
using allintegrations_factories.customers.parosa;
using allintegrations_factories.customers.partypek;
using apirest;
using BirokratNext;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.customers.spicasport;
using core.logic.common_birokrat;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using core.structs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using webshop_client_shopify;
using webshop_client_woocommerce;
using System.Threading.Tasks;

namespace core.customers {

    public class PredefinedIntegrationFactory : IIntegrationFactory
    {

        
        /*
         Proxy - the only responsibility is assigning Types to the underlying factory to fit the new pattern.
         */


        UnderlyingFactory underlyingFactory;
        public PredefinedIntegrationFactory(bool debug,
            string bironextAddress,
            string datafolder,
            string pythonpath = null,
            IOutApiClient enforcedApiClient = null,
            bool enforceWooToBiro = true,
            bool enforceBiroToWoo = true)
        {
            this.underlyingFactory = new UnderlyingFactory(debug, bironextAddress, datafolder,
                pythonpath, enforcedApiClient,
                enforceWooToBiro, enforceBiroToWoo);
        }

        public async Task<List<LazyIntegration>> GetAllLazy()
        {
            return (await underlyingFactory.GetAllLazy()).Select(x => Fix(x)).ToList();
        }
        public async Task<LazyIntegration> GetLazy(string key)
        {
            return Fix(await underlyingFactory.GetLazy(key));
        }
        public async Task<LazyIntegration> GetLazyByName(string name)
        {
            return Fix(await underlyingFactory.GetLazyByName(name));
        }

        private LazyIntegration Fix(LazyIntegration integ) {
            if (integ.Name.Contains("BIROTOWOO"))
            {
                integ.Type = "BIROTOWOO";
            }
            else if (integ.Name.Contains("WOOTOBIRO"))
            {
                integ.Type = "WOOTOBIRO";
            }
            else {
                throw new System.Exception("From undelying factory, there can be no integrations where name does not contain BIROTOWOO or WOOTOBIRO: " + integ.Name);
            }
            return integ;
        }
    }
    public class UnderlyingFactory : IIntegrationFactory
    {

        List<LazyIntegration> collection;
        bool debug;
        string datafolder;
        string pythonpath = null;

        IOutApiClient enforcedApiClient;
        
        public UnderlyingFactory(bool debug, 
            string bironextAddress, 
            string datafolder, 
            string pythonpath = null,
            IOutApiClient enforcedApiClient = null,
            bool enforceWooToBiro = true,
            bool enforceBiroToWoo = true) {
            List<LazyIntegration> integrations = new List<LazyIntegration>();

            this.pythonpath = pythonpath; 
            this.datafolder = datafolder;

            List<Task> tasks = new List<Task>();

            //Parosa(integrations, bironextAddress);
            //ShopifyTrgovina(integrations, bironextAddress);

            tasks.Add(new Task(() => TestnaTrgovina(integrations, bironextAddress)));

            foreach (var x in tasks) {
                x.Start();
            }
            Task.WhenAll(tasks).Wait();

            collection = integrations;
            this.debug = debug;

            this.enforcedApiClient = enforcedApiClient;
            this.enforceBiroToWoo = enforceBiroToWoo;
            this.enforceWooToBiro = enforceWooToBiro;
        }


        bool enforceWooToBiro;
        bool enforceBiroToWoo;
        IOutApiClient Wrap(IOutApiClient x, string integName) {

            bool wootobiro = false;
            if (integName.Contains("WOOTOBIRO"))
            {
                wootobiro = true;
            }
            else if (integName.Contains("BIROTOWOO"))
            {
                wootobiro = false;
            }
            else {
                throw new System.Exception("Integ name must contain WOOTOBIRO or BIROTOWOO!");
            }

            if (enforcedApiClient == null)
                return x;

            if (wootobiro && enforceWooToBiro)
                return enforcedApiClient;

            if (!wootobiro && enforceBiroToWoo)
                return enforcedApiClient;

            return x;
        }

        void TestnaTrgovina(List<LazyIntegration> integrations, string bironextAddress)
        {

            string address = "https://konecreative.eu/";
            string ck = "ck_24487f6811af50e42fd7dd709743ba3b0d428272";
            string cs = "cs_ba5ac93395728c97eb74dbc7e775f23e708abb47";
            string version = "wc/v3";
            var x = new WoocommerceRESTPythonCaller(address, ck, cs, version, 2, pythonpath);
            var wooclient = new WooApiClient(new WoocommerceCaller_NetworkFailureGuard(5, x));

            var storeKey = "dMepN2wPHm2pK/VAX8I1DyJ/PQsAQoByCsByMP+CTAA=";
            var storeName = "KONE_BIROTOWOO_PRODUCTION";
            integrations.Add(new LazyIntegration()
            {
                Name = storeName,
                Key = storeKey,
                BuildIntegrationAsync = async () => {
                    string biroApiKey = storeKey;
                    var client = new ApiClientV2(bironextAddress, biroApiKey);
                    return await new KoneshopIntegrationFactory(debug, datafolder)
                                .BuildIntegration(client, Wrap(wooclient, storeName),
                        biroApiKey, false, storeName, null);
                }
            });


            var x1 = new WoocommerceRESTPythonCaller(address, ck, cs, version, 2, pythonpath);
            var wooclient1 = new WooApiClient(new WoocommerceCaller_NetworkFailureGuard(5, x1));
            var storeKey1 = "SO3onPC7AhmrSgI54J6uNDKEfYmFpJlk+Ze7nskPQQw="; // newkey
            var storeName1 = "KONE_WOOTOBIRO_PRODUCTION";
            integrations.Add(new LazyIntegration()
            {
                Name = storeName1,
                Key = storeKey1,
                BuildIntegrationAsync = async () => {
                    string biroApiKey = storeKey1;
                    var client = new ApiClientV2(bironextAddress, biroApiKey);
                    return await new KoneshopIntegrationFactory(debug, datafolder)
                                .BuildIntegration(client, Wrap(wooclient1, storeName1),
                        biroApiKey, false, storeName1, null);
                }
            });
        }

        public async Task<List<IIntegration>> GetAll() {
            var tasks = collection.Select(x => x.BuildIntegrationAsync()).ToList();
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async Task<List<LazyIntegration>> GetAllLazy() {
            return collection;
        }

        public async Task<IIntegration> Get(string key) {
            var integ = collection.Where(x => x.Key == key).Single();
            return await integ.BuildIntegrationAsync();
        }

        public async Task<IIntegration> GetByName(string name) {
            var integ = collection.Where(x => x.Name == name).Single();
            return await integ.BuildIntegrationAsync();
        }

        public async Task<LazyIntegration> GetLazy(string key) {
            return collection.Where(x => x.Key == key).Single();
        }

        public async Task<LazyIntegration> GetLazyByName(string key) {
            return collection.Where(x => x.Name == key).Single();
        }
    }
}