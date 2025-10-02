using BiroWoocommerceHub;
using BiroWooHub.logic.integration;
using core.customers;
using core.logic.common_woo;
using core.tools.zalogaretriever;
using core.zgeneric;
using gui_generator;
using gui_generator.api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gui_generator_integs.final_adapter
{

    public interface ILazyIntegrationAdapter
    {
        Task<IIntegration> AdaptFinal(CurrentValue content, string integrationType);
    }
    public interface ICredentialsSource
    {
        Task<string> GetApiKey(int integrationId, string purpose, int taskId);
    }
    public class OnIntegrationLoad
    {

        OutClientEnforcingParameters enfparams;
        public OnIntegrationLoad(OutClientEnforcingParameters enfparams)
        {
            this.enfparams = enfparams;
        }

        public IMockWithInject<LazyIntegration> OnLoad(IMockWithInject<LazyIntegration> integ)
        {
            if (integ.GetSignature().Contains("BIROTOWOO") && enfparams.enforceBiroToWoo ||
                    integ.GetSignature().Contains("WOOTOBIRO") && enfparams.enforceWooToBiro)
            {
                integ.Inject(new Dictionary<string, object>
                    {
                        { "ioutapiclient", enfparams.enforcedClient }
                    }
                );
            }
            return integ;
        }
    }

    public class LazyIntegrationAdapterBuilder {

        public LazyIntegrationAdapterBuilder() { 
        
        }


        OutClientEnforcingParameters parameters = null;
        public LazyIntegrationAdapterBuilder withEnforcedParameters(OutClientEnforcingParameters parameters) {
            this.parameters = parameters;
            return this;
        }

        string integDataFolder = null;
        public LazyIntegrationAdapterBuilder withIntegDataFolder(string integDataFolder) {
            this.integDataFolder = integDataFolder;
            return this;
        }
        public string getIntegDataFolder() {
            if (integDataFolder == null)
                throw new ArgumentNullException(nameof(integDataFolder));
            return integDataFolder;
        }

        string bironext = null;
        public LazyIntegrationAdapterBuilder withBironext(string bironext) {
            this.bironext = bironext;
            return this;
        }

        string desiredName = null;
        public LazyIntegrationAdapterBuilder withDesiredName(string desiredName) {
            this.desiredName = desiredName;
            return this;
        }

        public ILazyIntegrationAdapter Create() {

            if (bironext == null)
                throw new ArgumentNullException("Cannot create lazy integration adapter without setting bironext address");
            if (integDataFolder == null)
                throw new ArgumentNullException("Cannot create lazy integration adapter without setting integDataFolder");
            if (parameters == null)
                throw new ArgumentNullException("Cannot create lazy integration adapter without out client enforcing params");
            //if (desiredName == null) NAME CAN REMAIN DEFAULT!
            //    throw new ArgumentNullException("Cannot create lazy integration adapter without desired name parameter");

            ILazyIntegrationAdapter tmp =  new FinalLazyIntegrationAdapter(integDataFolder, parameters);

            if (bironext != null) {
                tmp = new WithBironextAddress(bironext, tmp);
            }

            if (desiredName != null) { 
                tmp = new WithDesiredName(desiredName, tmp);
            }

            return tmp;
        }

    }

    internal class FinalLazyIntegrationAdapter : ILazyIntegrationAdapter
    {

        LazyIntegrationAdapter innerAdapter;
        string integrationType;
        string datafolder;
        OutClientEnforcingParameters enfparams;


        OnIntegrationLoad onLoad;
        public FinalLazyIntegrationAdapter(
            string datafolder,
            OutClientEnforcingParameters enfparams)
        {
            innerAdapter = new LazyIntegrationAdapter();
            this.datafolder = datafolder;
            this.enfparams = enfparams;
            onLoad = new OnIntegrationLoad(enfparams);
        }

        public async Task<IIntegration> AdaptFinal(CurrentValue content, string integrationType)
        {
            var integ = innerAdapter.Adapt(content, integrationType);
            integ = onLoad.OnLoad(integ);

            var tmp = integ.GetFields();

            LazyIntegration lazy = integ.Get();
            var integration = await lazy.BuildIntegrationAsync();
            handleInferredData(integration, tmp);

            return integration;
        }

        private void handleInferredData(IIntegration integration, Dictionary<string, object> variables)
        {
            var inferredData = innerAdapter.InferredData(integration);
            integration.Datafolder = Path.Combine(datafolder, integration.Name);
            InferDerivedComponents(integration, inferredData, variables);
        }

        private static void InferDerivedComponents(IIntegration integration, Dictionary<string, object> inferredData, Dictionary<string, object> variables)
        {

            // if the value is contained in variables, then use those with priority
            // if not, then use the inferred variables

            IZalogaRetriever parser = new NopZalogaRetriever();

            if (variables.ContainsKey("izalogaretriever") && variables["izalogaretriever"] != null) {
                parser = (IZalogaRetriever)variables["izalogaretriever"];
            }

            ICountryMapper countryMapper = new HardcodedCountryMapper();
            if (variables.ContainsKey("icountrymapper") && variables["icountrymapper"] != null) {
                countryMapper = (ICountryMapper)variables["icountrymapper"];
            }

            IVatIdParser vatidparser = new NopVatParser();
            if (variables.ContainsKey("ivatidparser") && variables["ivatidparser"] != null) {
                vatidparser = (IVatIdParser)variables["ivatidparser"];
            }

            List<string> statusList = new List<string>();
            if (inferredData.ContainsKey("statusList")) {
                statusList = (List<string>)inferredData["statusList"];
            }

            integration.ValidationComponents = new tests.tools.ValidationComponents(
                countryMapper,
                vatidparser,
                null,
                parser);
            integration.ObvezneNastavitve = new core.logic.common_birokrat.BirokratObvezneNastavitve(new Dictionary<string, string>());
            integration.TestingConfiguration = new TestingConfigParser().GetTestingConfig(integration, statusList);
        }
    }
}