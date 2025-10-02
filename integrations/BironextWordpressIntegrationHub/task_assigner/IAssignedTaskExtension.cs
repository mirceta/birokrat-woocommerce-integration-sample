using Microsoft.IdentityModel.Protocols;
using System.Threading.Tasks;

namespace task_assigner
{
    public interface IAssignedTaskExtension
    {
        string Path { get; }
        public Task<object> Execute(HttpRequestData request);
    }
}
