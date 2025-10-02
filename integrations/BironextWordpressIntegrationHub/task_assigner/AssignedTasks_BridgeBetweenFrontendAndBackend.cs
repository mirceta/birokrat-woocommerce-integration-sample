using administration_data.data.structs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using task_assigner;
using tasks;

namespace tasks
{

    public class AssignedTasks_BridgeBetweenFrontendAndBackend_Factory {

        ITaskAssigner _taskAssigner;
        public AssignedTasks_BridgeBetweenFrontendAndBackend_Factory(ITaskAssigner assigner) { 
            _taskAssigner = assigner;
        }

        public AssignedTasks_BridgeBetweenFrontendAndBackend Create(Delay<ITaskBuilder, string> delay, string taskName) {
            return new AssignedTasks_BridgeBetweenFrontendAndBackend(_taskAssigner, delay, taskName);
        }
    }

    public class AssignedTasks_BridgeBetweenFrontendAndBackend
    {
        ITaskAssigner _taskAssigner;
        Delay<ITaskBuilder, string> delay;
        string taskName;
        public AssignedTasks_BridgeBetweenFrontendAndBackend(ITaskAssigner taskAssigner, 
            Delay<ITaskBuilder, string> delay, 
            string taskName)
        {
            _taskAssigner = taskAssigner;
            this.delay = delay;
            this.taskName = taskName;
        }
        public async Task<AssignedTaskFrontendModel> Create(int integId, int? verId, AssignedTasksCreateRequest request)
        {
            var tmp = await _taskAssigner.Get(integId, verId, taskName);
            if (tmp != null)
                throw new Exception("Cannot create a new task when one already exists");

            var some = new Dictionary<string, string>();
            request.FormElements.ForEach(x => { some[x.name] = x.value; });
            var task = await _taskAssigner.Assign(integId, verId,
                            taskName, JsonConvert.SerializeObject(some));
            return await GetAssignedTask(integId, verId);
        }
        public async Task<AssignedTaskFrontendModel> GetAssignedTask(int integrationId, int? verId)
        {
            var tmp = await _taskAssigner.Get(integrationId, verId, taskName);
            // remark: null is a perfectly valid return type here, and prepareForFrontend may receive a null arg.
            return await (await delay.New(integrationId + "")).PrepareForFrontend(tmp);
        }
    }
    public class FormElement
    {
        public string name { get; set; }
        public string type { get; set; }
        public string label { get; set; }
        public string value { get; set; }

    }
    public class AssignedTaskFrontendModel
    {
        public string Status { get; set; }
        public List<FormElement> Form { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public string Program { get; set; }
    }
    public class AssignedTasksCreateRequest
    {
        public string IntegrationId { get; set; }
        public string VersionId { get; set; }
        public List<FormElement> FormElements { get; set; }
    }
}