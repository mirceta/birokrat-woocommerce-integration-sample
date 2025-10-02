using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using birowoo_exceptions;
using BiroWoocommerceHub;
using BiroWooHub.logic.integration;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using tests.tools;
using transfer_data_abstractions.orders;

namespace validator.logic
{
    public class UnverifiedOrderTransferProcessorStage : IOrderTransferProcessorStage {

        IOrderTransferAccessor accessor;
        IApiClientV2 bironext;
        ValidationComponents mapper;

        public UnverifiedOrderTransferProcessorStage(ValidationComponents mapper, IApiClientV2 bironext, IOrderTransferAccessor accessor) {
            if (accessor == null)
                throw new ArgumentNullException("accessor");
            if (bironext == null)
                throw new ArgumentNullException("bironext");
            if (mapper == null)
                throw new ArgumentNullException("mapper");
            this.accessor = accessor;
            this.bironext = bironext;
            this.mapper = mapper;
        }

        public async Task Handle(OrderTransfer orderTransfer) {

            var client = new ApiClientV3Document(bironext);
            await client.UpdateParameters(orderTransfer.BirokratDocType, orderTransfer.BirokratDocNum);
            await client.GetPdf(orderTransfer.BirokratDocType, orderTransfer.BirokratDocNum);
            string biroxml = await client.GetEslog(orderTransfer.BirokratDocType, orderTransfer.BirokratDocNum);
            
            if (string.IsNullOrEmpty(biroxml))
            {
                orderTransfer.Error = "Failed to retrieve birokrat document from Birokrat API. Unable to verify document.";
                orderTransfer.OrderTransferStatus = OrderTransferStatus.UNVERIFIED;
                await accessor.Set(orderTransfer);
                return;
            }


            string orderjson = await accessor.GetOrder(orderTransfer.OrderId);
            var order = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<WoocommerceOrder>(orderjson);

            try
            {
                var comparisonResult = await new WooOrderToBiroDocumentComparator().Compare(order, biroxml, mapper);
                if (comparisonResult.Success)
                {
                    orderTransfer.OrderTransferStatus = OrderTransferStatus.VERIFIED;
                }
                else
                {
                    orderTransfer.OrderTransferStatus = OrderTransferStatus.VERIFICATION_ERROR;
                    orderTransfer.Error = comparisonResult.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                orderTransfer.OrderTransferStatus = OrderTransferStatus.VERIFICATION_ERROR;
                orderTransfer.Error = "Unexpected error has occurred. Please inspect this document. " + ex.Message;
            }
            await accessor.Set(orderTransfer);
        }
    }
}