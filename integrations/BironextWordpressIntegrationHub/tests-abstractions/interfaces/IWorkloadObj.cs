using System;
using System.Threading;
using System.Threading.Tasks;

namespace tests
{
    public interface IWorkloadObj<T>
    {

        string Signature { get; set; }
        Task Execute(CancellationToken token);
        Task<string> GetInfo();
        T GetResult();

        Task<Exception> GetError();


    }
}
