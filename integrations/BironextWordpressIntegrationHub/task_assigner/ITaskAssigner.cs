using administration_data.data.structs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace task_assigner
{
    public interface ITaskAssigner
    {
        Task<AssignedTask> Assign(int integrationId, int? versionId, string taskName, string additionalParams);
        Task<AssignedTask> Get(int integrationId, int? versionId, string task);
    }

    public class HttpRequestData
    {
        public string Body { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public Dictionary<string, string> QueryStringParameters { get; set; }

        public HttpRequestData()
        {
            Headers = new Dictionary<string, string>();
            QueryStringParameters = new Dictionary<string, string>();
        }
    }

    public interface ITaskConsumer {
        void Subscribe();
        List<AssignedTask> GetChangedSince(DateTime dt);
        List<AssignedTask> GetAll();
        AssignedTask Get(int id);
        void Update(AssignedTask task);
    }
}
