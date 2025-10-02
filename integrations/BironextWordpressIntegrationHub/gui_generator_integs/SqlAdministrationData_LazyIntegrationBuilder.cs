using System.Collections.Generic;
using core.customers;
using administration_data.data.structs;
using administration_data;
using gui_generator;
using Newtonsoft.Json;
using gui_generator_integs.final_adapter;

namespace tests.composition.common
{
    public class SqlAdministrationData_LazyIntegrationBuilder
    {
        /*
         THIS SHOULD NEVER HAVE REFERENCES TO ASSIGNED TASKS!
         */

        private readonly string connectionString;
        private readonly ILazyIntegrationAdapter finalAdapter;

        public SqlAdministrationData_LazyIntegrationBuilder(string connectionString, ILazyIntegrationAdapter finalAdapter)
        {
            this.connectionString = connectionString;
            this.finalAdapter = finalAdapter;
        }

        public LazyIntegration Create(IntegrationVersion x)
        {
            var jsoncontent = new ContentDao(connectionString).Get(x.ContentId).Data;
            var integration = new IntegrationDao(connectionString).Get(x.IntegrationId);
            var content = JsonConvert.DeserializeObject<CurrentValue>(jsoncontent);
            var some = finalAdapter.AdaptFinal(content, integration.Type);
            var addinfo = new Dictionary<string, string>();
            addinfo["integrationId"] = x.IntegrationId + "";
            addinfo["versionId"] = x.Id + "";

            var lazy = new LazyIntegration()
            {
                Name = integration.Name,
                Key = integration.Name,
                Type = integration.Type,
                BuildIntegrationAsync = () => some,
                AdditionalInfo = addinfo
            };

            return lazy;
        }

        public LazyIntegration Create(IntegrationVersion x, string desiredName)
        {
            var jsoncontent = new ContentDao(connectionString).Get(x.ContentId).Data;
            var integration = new IntegrationDao(connectionString).Get(x.IntegrationId);
            var content = JsonConvert.DeserializeObject<CurrentValue>(jsoncontent);
            var some = finalAdapter.AdaptFinal(content, integration.Type);
            var addinfo = new Dictionary<string, string>();
            addinfo["integrationId"] = x.IntegrationId + "";
            addinfo["versionId"] = x.Id + "";

            var lazy = new LazyIntegration()
            {
                Name = desiredName,
                Key = desiredName,
                Type = integration.Type,
                BuildIntegrationAsync = () => some,
                AdditionalInfo = addinfo
            };

            return lazy;
        }
    }
}
