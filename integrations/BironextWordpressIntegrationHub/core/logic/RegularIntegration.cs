using BirokratNext;
using BiroWoocommerceHub;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.structs;
using gui_attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using tests.tools;
using validator.logic.order_transfer.accessor;

namespace core.customers.spicasport
{
    public class RegularIntegration : IIntegration {
        IApiClientV2 client;
        IOutApiClient wooclient;
        IBiroToWoo birotowoo;
        IWooToBiro wootobiro;
        bool includeValidator;
        BirokratObvezneNastavitve obvezneNastavitve;
        ICountryMapper countryMapper;

        string clientIdentifier;
        IBirokratPostavkaExtractor birokratPostavkaExtractor;
        private IntegrationIdentifier identifier;
        private string name;
        PhpPluginConfig config;
        private string datafolder;
        private ValidationComponents validationComponents;
        Options dodatneNastavitve;


        [GuiConstructor]
        public RegularIntegration(IApiClientV2 client,
            IOutApiClient wooclient,
            string name,
            IBiroToWoo birotowoo = null,
            IWooToBiro wootobiro = null,
            Options dodatneNastavitve = null) {


            // string clientIdentifier, -> should be injected!
            // string name -> this should be injected!
            includeValidator = true;

            if (client == null)
                throw new ArgumentNullException("client");
            if (wooclient == null)
                throw new ArgumentNullException("wooclient");

            this.client = client;
            this.wooclient = wooclient;
            SetBiroToWoo(birotowoo);
            SetWooToBiro(wootobiro);
            this.name = name;
            if (dodatneNastavitve != null)
                this.dodatneNastavitve = dodatneNastavitve;
            else
                this.dodatneNastavitve = new Options();
        }

        public RegularIntegration(IApiClientV2 client,
            IOutApiClient wooclient,
            string clientIdentifier,
            string name,
            IntegrationIdentifier identifier,
            BirokratObvezneNastavitve obvezneNastavitve,
            bool includeValidator = true,
            ValidationComponents validationComponents = null,
            IBiroToWoo birotowoo = null,
            IWooToBiro wootobiro = null,
            PhpPluginConfig config = null,
            string datafolder = "",
            Options dodatneNastavitve = null) {
            if (client == null)
                throw new ArgumentNullException("client");
            if (wooclient == null)
                throw new ArgumentNullException("wooclient");
            if (clientIdentifier == null)
                throw new ArgumentNullException("clientIdentifier");
            if (name == null)
                throw new ArgumentNullException("name");
            if (obvezneNastavitve == null)
                throw new ArgumentNullException("obvezneNastavitve");
            /*if (identifier == null) CURRENTLY NOT USED!
                throw new ArgumentNullException("identifier");*/


            if (validationComponents == null) {
                validationComponents = ValidationComponents.NullObject();
            }


            this.client = client;
            this.wooclient = wooclient;
            this.clientIdentifier = clientIdentifier;
            WooToBiroIdentifier = clientIdentifier;
            BiroToWooIdentifier = clientIdentifier;
            Name = name;
            Identifier = identifier;
            this.includeValidator = includeValidator;
            this.obvezneNastavitve = obvezneNastavitve;
            this.validationComponents = validationComponents;



            if (dodatneNastavitve != null)
                this.dodatneNastavitve = dodatneNastavitve;
            else
                this.dodatneNastavitve = new Options();

            SetBiroToWoo(birotowoo);
            SetWooToBiro(wootobiro);
            SetPhpConfig(config);
            this.datafolder = datafolder;
            CreateFolderHierarchy(datafolder);
        }

        public RegularIntegration SetBiroToWoo(IBiroToWoo birotowoo) {
            this.birotowoo = birotowoo;
            return this;
        }

        public RegularIntegration SetWooToBiro(IWooToBiro wootobiro) {
            this.wootobiro = wootobiro;
            return this;
        }

        public RegularIntegration SetPhpConfig(PhpPluginConfig config) {
            this.config = config;
            return this;
        }

        public RegularIntegration SetDatafolder(string datafolder) {
            this.Datafolder = datafolder;
            CreateFolderHierarchy(datafolder);
            return this;
        }

        public IBiroToWoo BiroToWoo {
            get {
                return birotowoo;
            }
            set => throw new NotImplementedException();
        }
        public IWooToBiro WooToBiro { get => wootobiro; set => throw new NotImplementedException(); }
        public string WooToBiroIdentifier { get; set; }
        public string BiroToWooIdentifier { get; set; }

        public IApiClientV2 BiroClient => client;

        public IOutApiClient WooClient { get => wooclient; set => wooclient = value; }

        public string Name { get => name; set => name = value; }


        public PhpPluginConfig PhpPluginConfigVal { get => config; set => config = value; }

        public string Datafolder { get; set; }

        public IBirokratPostavkaExtractor WooToBiroPostavkaExtractor { get => birokratPostavkaExtractor; set => birokratPostavkaExtractor = value; }

        public IntegrationIdentifier Identifier { get; set; }

        public bool IncludeValidator => includeValidator;

        public BirokratObvezneNastavitve ObvezneNastavitve { get => obvezneNastavitve; set => obvezneNastavitve = value; }

        public ValidationComponents ValidationComponents { get => validationComponents; set => validationComponents = value; }
        public Options Options => dodatneNastavitve;

        public TestingConfiguration TestingConfiguration { get; set; }


        Dictionary<string, string> externalInfo = new Dictionary<string, string>();
        public Dictionary<string, string> ExternalInfo { get => externalInfo; set => externalInfo = value; }

        private static void CreateFolderHierarchy(string path) {
            // handle case where it's a network path (\\path\...)

            if (string.IsNullOrEmpty(path)) return;

            string accum = "";
            string[] intermediates;
            if (path.Substring(0, 2) == "\\\\") {
                string newpath = path.Substring(2);
                intermediates = newpath.Split(new char[] { '\\', '/' });
                accum = path.Substring(0, 2 + intermediates[0].Length + 1);
                intermediates = intermediates.Skip(1).ToArray();
            } else {
                intermediates = path.Split(new char[] { '\\', '/' });
            }

            for (int i = 0; i < intermediates.Length; i++) {
                accum += intermediates[i] + "/";
                if (!Directory.Exists(accum))
                    Directory.CreateDirectory(accum);
            }
        }
    }

    
}

/*
 useful queries:
- get articles he wants to upload
SELECT TOP(1000) [alternativniopis]
      ,opis
      ,[artikel]
      ,[internetda]
	  ,[neuporabljaj]
      ,[barkoda3]
      ,[barkoda4]
      ,[barkoda5]
     
      ,[cenabrezdavka]
      ,[cenavvaluti]
      ,[cenazdavkom]
      ,[custom1]
      ,[custom2]

FROM[biro24394572 - 02#I].[dbo].[Artikel]
  
  
  where internetda = 1 and barkoda5 is not null and neuporabljaj != -1
  
  where internetda = 1 and barkoda5 is not null and neuporabljaj != -1
  order by artikel

 */