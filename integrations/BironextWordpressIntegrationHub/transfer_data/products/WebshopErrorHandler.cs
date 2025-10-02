using biro_to_woo_common.error_handling.errors;
using biro_to_woo_common.error_handling.reports;
using BirokratNext.api_clientv2;
using birowoo_exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tests_webshop.products;
using static System.Net.Mime.MediaTypeNames;

namespace transfer_data.products
{
    public class WebshopErrorHandler : IErrorHandler
    {

        IProductTransferAccessor accessor;
        public WebshopErrorHandler(IProductTransferAccessor accessor)
        {
            this.accessor = accessor;
        }

        public async Task HandleErrorList(List<IOperationReport> reports)
        {
            foreach (var x in reports)
                await HandleError(x);
        }

        public async Task HandleError(IOperationReport report)
        {
            ProductTransferSuccess outcome = ProductTransferSuccess.SUCCESSFUL;
            if (report.Ex == null)
            {
            }
            else if (report.Ex is IntegrationProcessingException)
            {
                outcome = ProductTransferSuccess.INTEGRATION_ERROR;
            }
            else
            {
                outcome = ProductTransferSuccess.INTERNAL_ERROR;
            }

            string message = "";
            if (report.Ex != null)
                message = report.Ex.Message + "_Signature:" + report.Signature + ":" + report.Ex.GetType();

            var mp = new Dictionary<string, ProductTransferEvent>() {
                { "SYNC", ProductTransferEvent.SYNC },
                { "ADD", ProductTransferEvent.ADD }
            };

            await accessor.AddOrUpdate(
                new ProductTransfer(report.Id, ProductTransferEvent.SYNC, outcome, message, DateTime.Now)
            );

        }

        public async Task HandleError(string signature, Exception excep)
        {

            string sifra = signature;
            try
            {
                if (excep != null)
                    throw excep;
                await accessor.AddOrUpdate(
                    new ProductTransfer(sifra, ProductTransferEvent.SYNC, ProductTransferSuccess.SUCCESSFUL, "", DateTime.Now));
                Console.WriteLine($"{sifra} success!");
            }
            catch (ProductAddingException ex)
            {
                await accessor.AddOrUpdate(
                    new ProductTransfer(sifra, ProductTransferEvent.ADD,
                    ProductTransferSuccess.INTEGRATION_ERROR, GetErrorMessage(ex), DateTime.Now));
            }
            catch (ProductUpdatingException ex)
            {
                await accessor.AddOrUpdate(
                        new ProductTransfer(sifra, ProductTransferEvent.SYNC,
                        ProductTransferSuccess.INTEGRATION_ERROR, GetErrorMessage(ex), DateTime.Now));
            }
            catch (ProductStillDifferentThanArtikelAfterUpdateException ex)
            {
                await accessor.AddOrUpdate(
                        new ProductTransfer(sifra, ProductTransferEvent.SYNC,
                        ProductTransferSuccess.INTEGRATION_ERROR, GetErrorMessage(ex), DateTime.Now));
            }
            catch (SifrantRecordNotFoundException ex)
            {
                await accessor.AddOrUpdate(
                        new ProductTransfer(sifra, ProductTransferEvent.SYNC,
                        ProductTransferSuccess.INTEGRATION_ERROR, GetErrorMessage(ex), DateTime.Now));
            }
            catch (IntegrationProcessingException ex)
            {
                await accessor.AddOrUpdate(
                        new ProductTransfer(sifra, ProductTransferEvent.SYNC,
                        ProductTransferSuccess.INTEGRATION_ERROR, GetErrorMessage(ex), DateTime.Now));
            }
            catch (Exception ex)
            {
                await accessor.AddOrUpdate(
                        new ProductTransfer(sifra, ProductTransferEvent.SYNC,
                        ProductTransferSuccess.INTERNAL_ERROR, GetErrorMessage(ex), DateTime.Now));
            }
        }

        private static string GetErrorMessage(Exception ex)
        {

            string message = ex.GetType().ToString() + ":" + ex.Message;
            if (ex.InnerException != null)
            {
                message += ex.InnerException.GetType().ToString();
                message += ex.InnerException.Message;
            }
            Console.WriteLine(message);
            return message;
        }
    }
}