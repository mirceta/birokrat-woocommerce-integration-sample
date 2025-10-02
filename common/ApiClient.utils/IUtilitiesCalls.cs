using BirokratNext.api_clientv2;
using System.Threading.Tasks;

namespace BirokratNext
{
    public interface IUtilitiesCalls : IMyLoggable
    {
        Task<string> DavcnaStevilka(string path, string davcna);
    }
}