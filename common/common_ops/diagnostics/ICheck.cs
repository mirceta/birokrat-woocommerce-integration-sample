using System.Threading.Tasks;

namespace common_ops.diagnostics
{
    public interface ICheck
    {
        Task<ResultRecord> Run();
    }
}
