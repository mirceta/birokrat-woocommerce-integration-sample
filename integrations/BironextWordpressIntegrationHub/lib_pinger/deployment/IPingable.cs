using System.Threading.Tasks;

namespace infrastructure_pinger.deployment
{
    interface IPingable
    {
        Task<string> Ping();
    }
}
