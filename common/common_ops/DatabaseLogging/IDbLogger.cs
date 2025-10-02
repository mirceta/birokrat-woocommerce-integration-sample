using System.Threading.Tasks;

namespace common_ops.DatabaseLogging
{
    public interface IDbLogger
    {
        Task LogAsync(params string[] data);
    }
}
