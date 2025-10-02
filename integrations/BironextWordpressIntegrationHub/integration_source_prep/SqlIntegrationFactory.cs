using administration_data;
using administration_data.data.structs;
using apirest;
using core.customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tests.composition.common;

namespace tests.composition.fixed_task.common
{

    public interface IVersionPicker
    {
        List<IntegrationVersion> GetAll();

        IntegrationVersion Get(string key);

        List<string> DetectChanges(DateTime since);
    }

    public class ProductionVersionPicker : IVersionPicker
    {

        IntegrationDao integDao;
        IntegrationVersionDao versionDao;
        string correctStatusName = "APPROVED";
        public ProductionVersionPicker(IntegrationDao integDao,
            IntegrationVersionDao versionDao)
        {
            this.integDao = integDao;
            this.versionDao = versionDao;
        }

        public List<string> DetectChanges(DateTime since)
        {
            var changedVersions = versionDao.GetVersionsChangedSince(since);
            var allIntegrations = integDao.GetAll(); // I'm assuming you have a method to fetch all BuildIntegrationAsync records.
            var approvedNames = (from iv in changedVersions
                                 where iv.Status == "APPROVED"
                                 join i in allIntegrations on iv.IntegrationId equals i.Id
                                 select i.Name).ToList();
            return approvedNames;
        }

        public IntegrationVersion Get(string key)
        {
            int id = integDao.GetAll().Where(x => x.Name == key).Single().Id;
            var version = versionDao
                .GetByIntegrationId(id)
                .Where(x => x.Status == "APPROVED")
                .OrderByDescending(x => x.Id).First();
            return version;
        }

        public List<IntegrationVersion> GetAll()
        {
            // filter all product versions
            List<IntegrationVersion> productionVersions = versionDao.GetAll();
            productionVersions = productionVersions.GroupBy(x => x.IntegrationId)
            .Select(x => x.Where(x => x.Status == correctStatusName)
                            .Aggregate((max, current) => max.Id > current.Id ? max : current)
            ).ToList();
            return productionVersions;
        }

        class ChangeDetector
        {

        }
    }

    public class SqlIntegrationFactory : IIntegrationFactory
    {

        IVersionPicker versionPicker;
        SqlAdministrationData_LazyIntegrationBuilder lazyIntegrationBuilder;

        public SqlIntegrationFactory(IVersionPicker versionPicker,
            SqlAdministrationData_LazyIntegrationBuilder lazyIntegrationBuilder)
        {
            this.versionPicker = versionPicker;
            this.lazyIntegrationBuilder = lazyIntegrationBuilder;
        }

        public Task<List<LazyIntegration>> GetAllLazy()
        {
            var productionVersions = versionPicker.GetAll();
            var lazies = new List<LazyIntegration>() { };
            foreach (var x in productionVersions)
            {
                LazyIntegration lazy = lazyIntegrationBuilder.Create(x);

                lazy.AdditionalInfo["integrationId"] = x.IntegrationId + "";
                lazy.AdditionalInfo["versionId"] = x.Id + "";

                lazies.Add(lazy);
            }
            return Task.FromResult(lazies);
        }

        public Task<LazyIntegration> GetLazy(string key)
        {

            var version = versionPicker.Get(key); 
            var result = lazyIntegrationBuilder.Create(version); 
            return Task.FromResult(result); 
        }

        public Task<LazyIntegration> GetLazyByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}
