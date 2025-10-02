using apirest;
using BiroWooHub.logic.integration;
using core.customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace allintegrations {
    public class PreloadedIntegrationFactory : IIntegrationFactory {

        List<IIntegration> integrations;

        public PreloadedIntegrationFactory() {
        }

        public async Task Setup(IIntegrationFactory integrations) {
            List<LazyIntegration> x = (await integrations.GetAllLazy());
            List<Task<IIntegration>> z = x.Select(async y => await y.BuildIntegrationAsync()).ToList();
            this.integrations = (await Task.WhenAll(z)).ToList();
        }

        public IIntegration Get(string key) {
            return integrations.Where(x => x.WooToBiroIdentifier == key).Single();
        }

        public IIntegration GetByName(string key) {
            return integrations.Where(x => x.Name == key).Single();
        }

        public List<IIntegration> GetAll() {
            return integrations;
        }

        public async Task<List<LazyIntegration>> GetAllLazy() {
            return integrations.Select(x => new LazyIntegration() {
                Name = x.Name,
                Key = x.WooToBiroIdentifier,
                BuildIntegrationAsync = async () => x
            }).ToList();

        }

        public async Task<LazyIntegration> GetLazy(string key) {
            return (await GetAllLazy()).Where(x => x.Key == key).Single();
        }

        public async Task<LazyIntegration> GetLazyByName(string name) {
            throw new NotImplementedException();
        }
    }
}
