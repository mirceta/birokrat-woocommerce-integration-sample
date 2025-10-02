using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using administration_data.data.structs;
using birolang_parser;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using transfer_data.orders.sql_accessors;
using validator;

namespace tasks.order_tests
{
    public class OrderTestsTaskBuilder : ITaskBuilder
    {

        OrderTransferDao _dao;
        string integrationId;
        public OrderTestsTaskBuilder(OrderTransferDao dao, string integrationId)
        {
            _dao = dao;
            this.integrationId = integrationId;

        }

        public async Task<AssignedTaskFrontendModel> PrepareForFrontend(AssignedTask tmp)
        {
            if (tmp != null)
                return await existingTask(tmp);
            return nullTask();
        }

        private AssignedTaskFrontendModel nullTask()
        {
            var assignedTask = new AssignedTaskFrontendModel();
            assignedTask.Status = "NONE";
            assignedTask.Form = getForm("", "");
            assignedTask.Data = new Dictionary<string, object>() {
                { "postOrderTestsUrl", "integrations/tasks/createAssignedTask?type=ORDERTESTS"},
            };
            string program = @"
                Public Sub OnCreate()
                    renderForm(null)
                End Sub

                Public Sub Create()
                    result = createTask(postOrderTestsUrl)
                    reloadTask(result)
                End Sub
            ";
            string json = BirolangInterpreter.Interpret(program).ToString();
            assignedTask.Program = json;
            return assignedTask;
        }

        private async Task<AssignedTaskFrontendModel> existingTask(AssignedTask tmp)
        {
            var some = JsonConvert.DeserializeObject<Dictionary<string, string>>(tmp.AdditionalParameters);
            var lst = await _dao.GetAllByIntegrationId(int.Parse(integrationId));

            var assignedTask = new AssignedTaskFrontendModel();
            assignedTask.Status = tmp.Status;
            assignedTask.Form = getForm(some["SinceDate"], some["MaxOrders"]);
            assignedTask.Data = new Dictionary<string, object>() {
                { "orderTransfers", lst },
                { "postOrderTestsUrl", "integrations/tasks/createAssignedTask?type=ORDERTESTS"},
                { "getPdfUrl", "tasks/getpdf?integrationId=${integrationId}&birokratDocType=${selectedOrderTransfer.birokratDocType}&birokratDocNumber=${selectedOrderTransfer.birokratDocNum}"},
                { "getOrderTestsUrl", "integrations/tasks/getAssignedTask/${integrationId}?type=ORDERTESTS" }
            };
            string program = orderTestsProgram();
            string json = BirolangInterpreter.Interpret(program).ToString();
            assignedTask.Program = json;
            return assignedTask;
        }

        private List<FormElement> getForm(string sinceDate, string maxOrders) { 
            return new List<FormElement>() {
                new FormElement() { label = "Začetni datum", type = "datetime", name = "SinceDate", value = sinceDate },
                new FormElement() { label = "Maksimalno število naročil", type = "int", name = "MaxOrders", value = maxOrders }
            };
        }

        private static string orderTestsProgram() {
            return @"
                
                Public Sub OnCreate()
                    renderForm(null)
                    renderOrderTransfers(orderTransfers)
                End Sub

                Public Sub Refresh()
                    result = get(getOrderTestsUrl)
                    reloadTask(result)
                End Sub

                Public Sub Pdf()
                    result = get(getPdfUrl)
                    renderPdf(result)
                End Sub
                
            ";
        }
    }

}


