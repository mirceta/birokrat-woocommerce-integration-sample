using administration_data.data.structs;
using System.Threading.Tasks;

namespace tasks
{
    public interface ITaskBuilder
    {
        Task<AssignedTaskFrontendModel> PrepareForFrontend(AssignedTask task);
    }
}