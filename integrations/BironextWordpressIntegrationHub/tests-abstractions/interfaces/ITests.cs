using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tests
{
    public interface ITests<T>
    {
        public Task Work(CancellationToken token);
        public T GetResult();
    }
}
