using BiroWooHub.logic.integration;
using core.customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apirest
{
    public interface IIntegrationFactory 
    {
        Task<List<LazyIntegration>> GetAllLazy();
        Task<LazyIntegration> GetLazy(string key);
        Task<LazyIntegration> GetLazyByName(string name);
    }
}
