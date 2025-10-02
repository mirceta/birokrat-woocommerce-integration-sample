using apirest;
using BiroWooHub.logic.integration;
using core.customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace allintegrations {
    public class EnvironmentDependentIntegrationFactory : IIntegrationFactory {


        List<LazyIntegration> collection;
        
        public static async Task<IIntegrationFactory> BiroToWooProduction(IIntegrationFactory factory) {
            var some = (await factory.GetAllLazy()).Where(x => x.Name.Contains("PRODUCTION") && x.Name.Contains("BIROTOWOO")).ToList();
            return new EnvironmentDependentIntegrationFactory(some);
        }

        public static async Task<IIntegrationFactory> BiroToWooStaging(IIntegrationFactory factory) {
            var some = (await factory.GetAllLazy()).Where(x => x.Name.Contains("STAGING") && x.Name.Contains("BIROTOWOO")).ToList();
            return new EnvironmentDependentIntegrationFactory(some);
        }

        public static async Task<IIntegrationFactory> WooToBiroProduction(IIntegrationFactory factory) {
            var some = (await factory.GetAllLazy()).Where(x => x.Name.Contains("PRODUCTION") && x.Name.Contains("WOOTOBIRO")).ToList();
            return new EnvironmentDependentIntegrationFactory(some);
        }

        public static async Task<IIntegrationFactory> WooToBiroStaging(IIntegrationFactory factory) {
            var some = (await factory.GetAllLazy()).Where(x => x.Name.Contains("STAGING") && x.Name.Contains("WOOTOBIRO")).ToList();
            return new EnvironmentDependentIntegrationFactory(some);
        }

        public static async Task<IIntegrationFactory> ValidatorProduction(IIntegrationFactory factory) {
            var some = (await factory.GetAllLazy()).Where(x => x.Name.Contains("PRODUCTION") && x.Name.Contains("VALIDATOR")).ToList();
            return new EnvironmentDependentIntegrationFactory(some);
        }

        private EnvironmentDependentIntegrationFactory(List<LazyIntegration> lazyints) {
            collection = lazyints;
        }

        public async Task<List<IIntegration>> GetAll()
        {
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

        public async Task<IIntegration> GetByName(string key) {
            var integ = collection.Where(x => x.Name == key).Single();
            return await integ.BuildIntegrationAsync();
        }

        public async Task<LazyIntegration> GetLazy(string key) {
            return collection.Where(x => x.Key == key).Single();
        }

        public async Task<LazyIntegration> GetLazyByName(string name) {
            throw new NotImplementedException();
        }
    }
}
