using administration_data;
using apirest;
using BiroWoocommerceHubTests;
using core.customers;
using gui_generator_integs.final_adapter;
using Microsoft.Extensions.Logging;
using si.birokrat.next.common.database;
using System;
using System.Linq;
using tests.composition.common;
using static Azure.Core.HttpHeader;


/*
 Story:
Problem importance reasoning:
Get bironext server under control. Bironext server and birokrat integrations are actually strongly related.
BIROWOO TESTS are tests for bironext server as well as birowoo project.
Birowoo tests should be faking metalno dobro odradjeno, because it is the most important project.
So BIROWOO TESTS ARE THE MOST IMPORTANT GOAL.
Problem specification:
For parameter list List<integration name> find all integrations they have.
Concurrently execute all of the tests for these firms.
Generate comprehensive reports on the tests so that you know exactly what works and what doesn’t.
Generate reports on how many times bironext was restarted and what errors occurred during the time bironext was restarted.
Problem dissection:
Task 1: If failed order/product retry {PARAMETER} times, if still fail, then restart bironext! - this should be encapsulated into a failure handling class which is injected into relevant mechanisms.
If tests are running async for multiple firms - this needs to be synchronized.
Task 2:
The tests should run asynchronously.
This is rather simple, as in this case the tests are not looping but rather only executed one time. Thus you can just start a new thread and run it - the only shared resources are BironextResetter and DatabaseResetter.
Task 3:
Need another top level abstraction - MultipleTestDriver, SingleTestDriver.
MultipleTestDriver(List<IIntegration> integrations) { // can be both!
SingleTestDriver(IIntegration integration) // if BIROTOWOO … product tests else ordertests
Task 3:
For products: webshop must be either mocked, or we have multiple woocommerces.
Task 4: 
Test report generation: how do we know that we pass?
 */

namespace tests.composition.fixed_task.common
{
    public class IntegrationFactoryBuilder
    {

        string type = null;
        bool debug;
        IOutApiClient enforcedApiClient;
        bool enforceBiroToWoo;
        bool enforceWooToBiro;


        public IntegrationFactoryBuilder() { }

        public IntegrationFactoryBuilder ofType(string type) {
            this.type = type;
            return this;
        }

        string connectionString = null;
        public IntegrationFactoryBuilder withSqlServer(string connectionString) {
            this.connectionString = connectionString;
            return this;
        }
        public string getSqlServer() {
            return connectionString;
        }

        public IntegrationFactoryBuilder withOutClientOverriding(bool debug, IOutApiClient enforcedApiClient,
            bool enforceBiroToWoo, bool enforceWooToBiro)
        {
            this.debug = debug;
            this.enforcedApiClient = enforcedApiClient;
            this.enforceBiroToWoo = enforceBiroToWoo;
            this.enforceWooToBiro = enforceWooToBiro;
            return this;
        }

        string pythonPath = null;
        public IntegrationFactoryBuilder withOverridePythonPath(string pythonPath) {
            this.pythonPath = pythonPath;
            return this;
        }

        public IIntegrationFactory build(string bironextaddress, string integrationdatafolder) {

            if (type == "PRE")
            {
                var factory = new PredefinedIntegrationFactory(debug: true,
                        bironextaddress,
                        integrationdatafolder,
                        pythonPath,
                        enforcedApiClient: enforcedApiClient,
                        enforceWooToBiro: enforceWooToBiro,
                        enforceBiroToWoo: enforceBiroToWoo);
                return factory;
            }
            else if (type == "JSON")
            {

                if (connectionString == null) {
                    throw new Exception("Invalid configuration. JSON IntegrationFactoryBuilder type requires the SQLServer to be set!");
                }

                var adapterFactory = new LazyIntegrationAdapterBuilder();
                adapterFactory.withBironext(bironextaddress);
                adapterFactory.withEnforcedParameters(new OutClientEnforcingParameters()
                {
                    enforcedClient = null,
                    enforceBiroToWoo = false,
                    enforceWooToBiro = false
                });
                adapterFactory.withIntegDataFolder(integrationdatafolder);
                var adapter = adapterFactory.Create();

                var fac = new SqlIntegrationFactory(

                            /*this may later represent a problem - ProductionVersionPicker is constant again - so we will be unable to run tests
                             on DRAFT integrations, or other integrations. This seems to be mitigatable though, by just injecting the correct VersionPicker
                             into this class here, though this would then mandate that the ITests TASK is set before loading the integrations, and after
                             the integrations are loaded, the task should not be changeable anymore! */
                            new ProductionVersionPicker(
                                new administration_data.IntegrationDao(connectionString),
                                new administration_data.IntegrationVersionDao(connectionString)),
                            new SqlAdministrationData_LazyIntegrationBuilder(connectionString, adapter));
                return fac;
            }
            else {
                throw new Exception("Unrecognized type");
            }
        }

        public bool IsDangerous()
        {
            return enforcedApiClient == null || enforceBiroToWoo == false || enforceBiroToWoo == false;
        }

        private void nullCheck() {
            
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "Type must be set before building the object.");
            }
        }
    }
}
