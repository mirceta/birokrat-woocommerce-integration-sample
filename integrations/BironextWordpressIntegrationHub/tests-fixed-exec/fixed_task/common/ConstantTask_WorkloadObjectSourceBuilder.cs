using System;
using System.Collections.Generic;
using tests.tests.estrada;
using BiroWoocommerceHubTests;
using tests.composition.root_builder;
using Microsoft.Extensions.Logging;
using tests.interfaces;
using gui_generator.api;
using tests.composition.common;
using tests.composition.fixed_integration.fixed_task.fixed_workload_object_source;
using tests.composition.fixed_integration.looper;
using gui_generator_integs.final_adapter;
using Newtonsoft.Json.Linq;

namespace tests.composition.fixed_task.common
{

    public class ConstantTask_WorkloadObjectSourceBuilder : BaseWorkloadObjectSourceBuilder
    {
        private ITestsFactory source;
        private List<string> integNames;


        bool debug;
        IOutApiClient enforcedApiClient;
        bool enforceBiroToWoo;
        bool enforceWooToBiro;

        string additionalParams;

        public ConstantTask_WorkloadObjectSourceBuilder() : base()
        {
        }

        #region [additional with]
        public ConstantTask_WorkloadObjectSourceBuilder withOutClientOverriding(bool debug, IOutApiClient enforcedApiClient,
                bool enforceBiroToWoo, bool enforceWooToBiro)
        {
            this.debug = debug;
            this.enforcedApiClient = enforcedApiClient;
            this.enforceBiroToWoo = enforceBiroToWoo;
            this.enforceWooToBiro = enforceWooToBiro;
            return this;
        }
        public ConstantTask_WorkloadObjectSourceBuilder withSource(ITestsFactory source)
        {
            this.source = source;
            return this;
        }
        public ConstantTask_WorkloadObjectSourceBuilder withIntegNames(List<string> integNames)
        {
            this.integNames = integNames;
            return this;
        }
        public ConstantTask_WorkloadObjectSourceBuilder withAdditionalParams(string additionalParams)
        {
            this.additionalParams = additionalParams;
            return this;
        }

        IntegrationFactoryBuilder integrationFactoryBuilder = null;
        public ConstantTask_WorkloadObjectSourceBuilder withIntegrationFactoryBuilder(IntegrationFactoryBuilder builder) {
            this.integrationFactoryBuilder = builder;
            return this;        
        }
        #endregion

        bool alreadyEntered = false;
        public override IWorkloadObjectSource build()
        {
            nullChecks();

            integrationFactoryBuilder.withOutClientOverriding(debug: debug,
                            enforcedApiClient: enforcedApiClient,
                            enforceBiroToWoo: enforceBiroToWoo, // only this relevant.
                            enforceWooToBiro: enforceWooToBiro);

            return new FixedWorkloadObjectSource(loggerFactory, 
                source,
                testenv,
                integrationFactoryBuilder,
                bironextAddress,
                integrationDataFolder,
                integNames,
                additionalParams);
        }
        #region [auxiliary]
        void nullChecks()
        {

            if (alreadyEntered)
                throw new Exception("Only meant to build one object");
            alreadyEntered = true;

            base.build();
            if (integrationFactoryBuilder == null) {
                throw new ArgumentNullException(nameof(integrationFactoryBuilder), "Integration factory builder must be set before building the object.");
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Source must be set before building the object.");
            }
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory), "LoggerFactory must be set before building the object.");
            }
        }
        #endregion

    }
}
