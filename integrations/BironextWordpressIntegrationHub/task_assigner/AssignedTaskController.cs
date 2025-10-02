using System;
using System.Threading.Tasks;
using task_assigner;
using tasks;
using System.Text;
using si.birokrat.next.common.conversion;

namespace BironextWordpressIntegrationHub.Controllers
{
    public partial class AssignedTaskController

    {
        
        private readonly IAssignedTaskRegistry _registry;
        private readonly AssignedTasks_BridgeBetweenFrontendAndBackend_Factory _composerFactory;

        public AssignedTaskController(IAssignedTaskRegistry registry, 
            AssignedTasks_BridgeBetweenFrontendAndBackend_Factory composerFactory)
        {
            _registry = registry;
            _composerFactory = composerFactory;
        }

        public async Task<AssignedTaskFrontendModel> CreateTask(string info,
            AssignedTasksCreateRequest request,
            string type)
        {
            if (request == null)
                throw new Exception("Request body cannot be null.");

            var dinfo = HttpConverter.DecodeAndDeserialize<FDllInfo>(info, Encoding.UTF8);
            var taskFactory = _registry.Get(type);
            await taskFactory.BeforeCreate(dinfo, request);
            var taskBuilderFactory = new Delay<ITaskBuilder, string>(async (integrationId) => 
                            await taskFactory.Create(int.Parse(integrationId)));

            var composer = _composerFactory.Create(taskBuilderFactory, type);
            return await composer.Create(int.Parse(request.IntegrationId), request.VersionId == null ? null : (int?)int.Parse(request.VersionId), request);
        }

        public async Task<AssignedTaskFrontendModel> GetTask(int integrationId, int? verId, string type)
        {
            var core = _registry.Get(type);
            var chome = new Delay<ITaskBuilder, string>(async (integrationId) =>
                            await core.Create(int.Parse(integrationId)));
            var composer = _composerFactory.Create(chome, type);
            var result = await composer.GetAssignedTask(integrationId, verId);
            return result;
        }
    }

    public class DllInfo : Info
    {
        public string Token { get; set; } = string.Empty;

        public string DeviceIdentifier { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string PoslovnoLeto { get; set; } = string.Empty;

        public string Mode { get; set; } = string.Empty;

        public string ClientType { get; set; } = string.Empty;

        // DEPRECATED (LEFT FOR COMPATIBILITY REASONS)

        public int Fiscalization { get; set; } = 0;

        public string FilePath { get; set; } = string.Empty;

        public bool Global { get; set; } = false;

        public new string ToString()
        {
            string retval = "";
            if (!string.IsNullOrEmpty(Token))
            {
                retval += $"Token: {Token}";
            }
            if (!string.IsNullOrEmpty(DeviceIdentifier))
            {
                retval += $"DeviceIdentifier: {DeviceIdentifier}";
            }
            if (!string.IsNullOrEmpty(UserName))
            {
                retval += $"UserName: {UserName}";
            }
            if (!string.IsNullOrEmpty(Password))
            {
                retval += $"Password: {Password}";
            }
            if (!string.IsNullOrEmpty(PoslovnoLeto))
            {
                retval += $"PoslovnoLeto: {PoslovnoLeto}";
            }
            if (!string.IsNullOrEmpty(Mode))
            {
                retval += $"Mode: {Mode}";
            }
            // fiscalization
            retval += $"Fiscalization: {Fiscalization}";
            if (!string.IsNullOrEmpty(FilePath))
            {
                retval += $"FilePath: {FilePath}";
            }
            // global
            retval += $"Global: {Global.ToString()}";
            return retval;
        }
    }
    public class Info
    {
        public string TaxNumber { get; set; } = string.Empty;

        public string SqlServer { get; set; } = string.Empty;

        public string SqlPassword { get; set; } = string.Empty;
    }
}


