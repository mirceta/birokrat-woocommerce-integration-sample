using System.Threading.Tasks;
using tasks;
using bironext_autocredentials;
using task_assigned_frontend_generation.tasks.order_production;
using System.Collections.Generic;
using BironextWordpressIntegrationHub.Controllers;
using tasks.order_tests;
using pdf_handling;
using administration_data.data.structs;
using task_assigner;
using transfer_data.orders.sql_accessors;

namespace task_assigned_frontend_generation.tasks.order_tests
{

    public class OrderTestsFrontendFactory_Production {
        public OrderTestsFrontendFactory Create(string connString, string bironextAddress) {
            var upserter = new BironextApiKeyCredentialUpserterFactory(connString, bironextAddress);
            var dao = new OrderTransferDao(connString, "Tests");
            var getpdfextension = new GetPdfExtension(new PdfDataDao(connString));
            return new OrderTestsFrontendFactory(upserter, dao, getpdfextension);
        }
    }
    public class OrderTestsFrontendFactory : IAssignedTaskFrontendFactory
    {
        IBironextApiKeyCredentialUpserterFactory apiKeysFactory;
        OrderTransferDao orderTransferDao;
        GetPdfExtension getPdfExtension;
        public OrderTestsFrontendFactory(IBironextApiKeyCredentialUpserterFactory apiKeysFactory,
                                         OrderTransferDao orderTransferDao,
                                         GetPdfExtension getPdfExtension)
        {
            this.apiKeysFactory = apiKeysFactory;
            this.orderTransferDao = orderTransferDao;
            this.getPdfExtension = getPdfExtension;
        }
        public async Task BeforeCreate(FDllInfo dinfo, AssignedTasksCreateRequest request)
        {
            var upserter = apiKeysFactory.Create(dinfo.Token);
            await upserter.EnsureKeysAsync(5, "PRODUCTION", int.Parse(request.IntegrationId));
        }
        public async Task<ITaskBuilder> Create(int integrationId)
        {
            var some = new OrderTestsTaskBuilder(
                orderTransferDao,
                integrationId + ""
            );
            return some;
        }

        public List<IAssignedTaskExtension> GetExtensions()
        {
            var extensions = new List<IAssignedTaskExtension>();
            extensions.Add(getPdfExtension);
            return extensions;
        }
    }

    public class GetPdfExtension : IAssignedTaskExtension
    {
        public string Path => "getpdf/{0}";


        PdfDataDao dao;
        public GetPdfExtension(PdfDataDao dao) { 
            this.dao = dao;
        }

        public async Task<object> Execute(HttpRequestData request)
        {
            string birokratDocType = (string)request.QueryStringParameters["birokratDocType"];
            string birokratDocNumber = (string)request.QueryStringParameters["birokratDocNumber"];
            string integrationId = (string)request.QueryStringParameters["integrationId"];
            var result = await dao.GetAsync(int.Parse(integrationId), birokratDocType.ToLower(), birokratDocNumber);
            return result;
        }

    }
}




