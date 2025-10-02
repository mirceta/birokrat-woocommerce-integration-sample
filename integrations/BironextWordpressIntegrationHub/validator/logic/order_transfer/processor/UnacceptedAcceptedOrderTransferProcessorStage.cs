using ApiClient.utils;
using BirokratNext.api_clientv2;
using BirokratNext.Exceptions;
using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using core.customers.zgeneric;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using transfer_data_abstractions.orders;
using validator.logic.order_transfer.accessor;

namespace validator.logic
{



    public class UnacceptedAcceptedOrderTransferProcessorStage : IOrderTransferProcessorStage {

        IOrderTransferAccessor accessor;
        IWooToBiro wootobiro;
        IOrderPostprocessor orderPostprocessor;

        public UnacceptedAcceptedOrderTransferProcessorStage(IWooToBiro wootobiro,
            IOrderTransferAccessor accessor,
            IOrderPostprocessor orderPostprocessor) {
            if (wootobiro == null)
                throw new ArgumentNullException("wootobiro");
            if (accessor == null)
                throw new ArgumentNullException("accessor");
            if (orderPostprocessor == null)
                throw new ArgumentNullException("orderPostprocessor");
            this.accessor = accessor;
            this.wootobiro = wootobiro;
            this.orderPostprocessor = orderPostprocessor;
        }

        public async Task Handle(OrderTransfer orderTransfer) {

            try {
                orderTransfer.DateLastModified = DateTime.Now;
                string order = await accessor.GetOrder(orderTransfer.OrderId);

                // the processing status of the order should be changed to what we are expecting 
                // as it's possible that it has gone on to other states!
                var odr = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<WoocommerceOrder>(order);
                odr = orderPostprocessor.Postprocess(odr);


                odr.Data.Status = orderTransfer.OrderStatus;
                order = JsonConvert.SerializeObject(odr);


                var result = await wootobiro.OnOrderStatusChanged(order);
                string documentType = (string)result["tipDokumenta"];
                string documentNumber = (string)result["stevilkaDokumenta"];

                try {
                    orderTransfer = new OrderTransferBuilder().Success(orderTransfer, result);
                    await accessor.Set(orderTransfer);
                } catch (Exception ex) {
                    orderTransfer = new OrderTransferBuilder().Unassigned(orderTransfer);
                    await accessor.Set(orderTransfer);
                }
                //return Ok(result); // result contains stevilka dokumenta!
            } catch (BironextApiCallException ex) {
                orderTransfer.OrderTransferStatus = OrderTransferStatus.ERROR; // THIS IS NOT SAFE - AS IT CAN BE A BAD REQUEST EXCEPTION OR AN INTERNAL SERVER ERROR
                // IF IT'S A BAD REQUEST THERE'S NO POINT IN RETRYING, WHILE THERE IS A POINT IN RETRYING IF ITS INTERNAL SERVER ERROR!
                orderTransfer.Error = "Birokrat error:" + ex.Message;
                await accessor.Set(orderTransfer);
            } catch (BironextRestartException ex) { // RESTART EXCEPTION IS ACTUALLY SAFER - WE KNOW THAT BIROKRAT WORKED BUT THAT AN ERROR OCCURED - ON THIS OCCASION WE MAY HAVE A COUNTER HERE - TRY 3 TIMES DURING BIROKRAT RESTART UNTIL GIVING UP!
                if (orderTransfer.OrderTransferStatus == OrderTransferStatus.UNVERIFIED) return;
                orderTransfer.OrderTransferStatus = OrderTransferStatus.ERROR;
                await accessor.Set(orderTransfer);
            } catch (OrderFlowOperationNotFoundException ex) {
                orderTransfer.OrderTransferStatus = OrderTransferStatus.NO_EVENT;
                await accessor.Set(orderTransfer);
            } catch (Exception ex) {
                orderTransfer.OrderTransferStatus = OrderTransferStatus.ERROR;
                orderTransfer.Error = ex.Message;
                await accessor.Set(orderTransfer);
            }
        }

        
    }

    public class OrderTransferBuilder {
        public OrderTransferBuilder() { }


        public OrderTransfer Success(OrderTransfer orderTransfer, Dictionary<string, object> result) {
            string documentType = (string)result["tipDokumenta"];
            string documentNumber = (string)result["stevilkaDokumenta"];
            orderTransfer.BirokratDocNum = documentNumber;
            orderTransfer.BirokratDocType = Reverse(BironextApiPathHelper.biroDoctTypeMap)[documentType];
            orderTransfer.OrderTransferStatus = OrderTransferStatus.UNVERIFIED;
            return orderTransfer;
        }

        public OrderTransfer Unassigned(OrderTransfer orderTransfer) {
            orderTransfer.BirokratDocNum = "0000";
            orderTransfer.BirokratDocType = BirokratDocumentType.UNASSIGNED;
            orderTransfer.OrderTransferStatus = OrderTransferStatus.VERIFICATION_ERROR;
            return orderTransfer;
        }

        public static Dictionary<TValue, TKey> Reverse<TKey, TValue>(IDictionary<TKey, TValue> source)
        {
            var dictionary = new Dictionary<TValue, TKey>();
            foreach (var entry in source)
            {
                if (!dictionary.ContainsKey(entry.Value))
                    dictionary.Add(entry.Value, entry.Key);
            }
            return dictionary;
        }
    }
}