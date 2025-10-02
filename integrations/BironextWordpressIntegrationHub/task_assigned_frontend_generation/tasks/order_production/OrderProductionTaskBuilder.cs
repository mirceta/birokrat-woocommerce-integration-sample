using System.Collections.Generic;
using System.Threading.Tasks;
using administration_data.data.structs;
using birolang_parser;
using Newtonsoft.Json;
using tasks;
using transfer_data.orders.sql_accessors;

namespace task_assigned_frontend_generation.tasks.order_production
{
    public class OrderProductionTaskBuilder : ITaskBuilder
    {

        OrderTransferDao _dao;
        string integrationId;
        public OrderProductionTaskBuilder(OrderTransferDao dao, string integrationId)
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
                { "postOrderProdUrl", "api/tasks/createOrderProductionTask"},
            };
            assignedTask.Program = @"
                Public Sub OnCreate()
                    renderForm(null)
                End Sub

                Public Sub Create()
                    some = packForm(form)
                    result = post(postOrderProdUrl, some)
                    reloadTask(result)
                End Sub
            ";
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
                { "postOrderTestsUrl", "api/tasks/createTestsTask"},
                { "getPdfUrl", "api/tasks/getpdf?integrationId=${integrationId}&birokratDocType=${selectedOrderTransfer.birokratDocType}&birokratDocNumber=${selectedOrderTransfer.birokratDocNum}"},
                { "getOrderTestsUrl", "api/tasks/getAssignedTestsTask/${integrationId}"}
            };
            string program = orderTestsProgram();
            string json = BirolangInterpreter.Interpret(program).ToString();
            assignedTask.Program = json;
            return assignedTask;
        }

        private List<FormElement> getForm(string neverLookBeforeDate, string timeWindowDays)
        {
            return new List<FormElement>() {
                new FormElement() { label = "Časovno okno v dnevih",              type = "int",      name = "MaxOrders",           value = timeWindowDays },
                new FormElement() { label = "Ne obravnavaj naročil pred datumom", type = "datetime", name = "NeverLookBeforeDate", value = neverLookBeforeDate }
            };
        }

        private static string orderTestsProgram()
        {
            return @"
                
                Public Sub OnCreate()
                    renderForm(null)
                    renderOrderTransfers(orderTransfers)
                End Sub

                Public Sub Refresh()
                    result = get(getOrderTestsUrl)
                    reloadTask(result)
                End Sub
                
            ";
        }

    }

}


