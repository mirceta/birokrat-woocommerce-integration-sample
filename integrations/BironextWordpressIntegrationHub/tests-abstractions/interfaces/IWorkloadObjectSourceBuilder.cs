using tests.tests.estrada;
using tests.composition.common;
using System;
using tests.composition.root_builder;

namespace tests.interfaces
{
    public interface IWorkloadObjectSourceBuilder
    {
        IWorkloadObjectSourceBuilder withIntegrationDataFolder(string integrationDataFolder);
        IWorkloadObjectSourceBuilder withTestenv(TestEnvironmentParams testenv);
        IWorkloadObjectSourceBuilder withBironextAddress(string bironextAddress);
        IWorkloadObjectSource build();
    }

    public abstract class BaseWorkloadObjectSourceBuilder : IWorkloadObjectSourceBuilder
    {

        protected string bironextAddress;
        protected string integrationDataFolder;
        protected TestEnvironmentParams testenv;
        protected IMyLoggerFactory loggerFactory;

        public BaseWorkloadObjectSourceBuilder() {}

        public virtual IWorkloadObjectSource build() {
            if (bironextAddress == null)
            {
                throw new ArgumentNullException(nameof(bironextAddress), "BironextAddress must be set before building the object.");
            }

            if (integrationDataFolder == null)
            {
                throw new ArgumentNullException(nameof(integrationDataFolder), "IntegrationDataFolder must be set before building the object.");
            }
            if (loggerFactory == null) {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            if (testenv == null && !allowNoTestEnv) {
                throw new ArgumentNullException(nameof(testenv));
            }
            return null;
        }

        bool allowNoTestEnv = false;
        public void AllowNoTestEnv() {
            allowNoTestEnv = true;
        }

        public IWorkloadObjectSourceBuilder withBironextAddress(string bironextAddress)
        {
            this.bironextAddress = bironextAddress;
            return this;
        }
        public IWorkloadObjectSourceBuilder withTestenv(TestEnvironmentParams testenv)
        {
            this.testenv = testenv;
            return this;
        }

        public IWorkloadObjectSourceBuilder withIntegrationDataFolder(string integrationDataFolder)
        {
            this.integrationDataFolder = integrationDataFolder;
            return this;
        }
        public BaseWorkloadObjectSourceBuilder withLoggerFactory(IMyLoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            return this;
        }
    }
}
