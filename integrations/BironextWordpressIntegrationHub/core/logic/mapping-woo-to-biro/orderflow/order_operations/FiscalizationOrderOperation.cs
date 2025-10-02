using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using birowoo_exceptions;
using core.customers.zgeneric;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using tests.tools;

namespace core.logic.mapping_woo_to_biro.orderflow.order_operations
{
    public class FiscalizationOrderOperation : IOrderOperationCR
    {

        IApiClientV2 client;
        ValidationComponents mapper;
        IOrderOperationCR next;

        public FiscalizationOrderOperation(IApiClientV2 client, ValidationComponents mapper, IOrderOperationCR next) {
            this.client = client;
            this.mapper = mapper;
            this.next = next;

            // verify that fiscalization settings are OK:

            // BIROKRAT NEEDS TO MAKE A NEW API CALL!
        }

        public async Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data) {

            string stevilkaDokumenta = (string)data["stevilkaDokumenta"];
            string tipDokumenta = (string)data["tipDokumenta"];


            var client3 = new ApiClientV3Document(client);
            var type = BironextApiPathHelper.GetTypeByString(tipDokumenta);
            string biroxml = await client3.GetEslog(type, stevilkaDokumenta);

            try {
                await new WooOrderToBiroDocumentComparator().Compare(order, biroxml, mapper);
            } catch (PriceValidationException ex) {
                throw new IntegrationProcessingException($"{errorMessage()} {ex.Message}");
            } catch (DavcnaValidationException ex) {
                throw new IntegrationProcessingException($"{errorMessage()} {ex.Message}");
            } catch (ValidationException ex) { 
                // other validation exceptions are permitted
            }

            // after here, the racun is validated


            try {
                string result = await client3.Fiscalize(type, stevilkaDokumenta);
                if (result != "OK") {
                    throw new Exception($"Birokrat has returned an error during fiscalization: {result}");
                }
            } catch (Exception ex) {
                throw new IntegrationProcessingException($"Error during fiscalization: {ex.Message}");
            }

            if (next != null) {
                return await next.Next(order, data);
            }
            return data;
        }

        private static string errorMessage() {
            return @"
Before fiscalization: during validation of the order, a mismatch was found between the order and the
birokrat document. A validated document is the requirement for fiscalization, so fiscalization was terminated.
            ";
        }
    }
}
