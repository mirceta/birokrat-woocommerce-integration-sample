using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion
{
    public class DocumentInsertion
    {

        BirokratDocumentType documentType;
        IApiClientV2 client;
        IBirokratPostavkaExtractor postavkaExtractor;
        List<IAdditionalOperationOnPostavke> additionalPostavkeOps;

        ICountryMapper countryMapper;

        Func<WoocommerceOrder, OrderAdditionalParams> additionalParamsFunction = null;

        public DocumentInsertion(IApiClientV2 client, BirokratDocumentType documentType, 
            IBirokratPostavkaExtractor postavkaExtractor,
            List<IAdditionalOperationOnPostavke> additionalPostavkeOps,
            ICountryMapper countryMapper = null) {

            this.client = client;
            this.documentType = documentType;
            this.postavkaExtractor = postavkaExtractor;
            this.additionalPostavkeOps = additionalPostavkeOps;
            this.countryMapper = countryMapper;
        }

        public DocumentInsertion SetAdditionalParams(Func<WoocommerceOrder, OrderAdditionalParams> additionalParamsFunction) {
            this.additionalParamsFunction = additionalParamsFunction;
            return this;
        }

        public async Task<string> InsertDocument(WoocommerceOrder order, string billingBirokratId = null) {

            var postavke = await postavkaExtractor.ExtractFromOrder(order);

            foreach (var couponHandler in additionalPostavkeOps)
                postavke = await couponHandler.ApplyOperationToPostavke(order, postavke);

            OrderAdditionalParams pars = null;
            if (additionalParamsFunction == null)
                pars = OrderAdditionalParams.BuildDefaultWithCountryMapper(countryMapper, order);
            else
                pars = additionalParamsFunction(order);
            pars.BirokratId = billingBirokratId;

            string json = await SimplejsonGen.CreateJsonDocumentRequest(order, postavke,
                pars);
            
            string result = await client.document.CreateSimpleJson(BironextApiPathHelper.GetVnosByType(documentType), json);

            return result;

        }
    }
}
