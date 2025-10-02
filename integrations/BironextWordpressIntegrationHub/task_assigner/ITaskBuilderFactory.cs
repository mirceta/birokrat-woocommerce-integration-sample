using System;
using System.Threading.Tasks;

namespace tasks
{
    public class Delay<T, U>
    {
        private Func<U, Task<T>> _func;
        public Delay(Func<U, Task<T>> func)
        {
            this._func = func;
        }
        public async Task<T> New(U parameter)
        {
            return await _func(parameter);
        }
    }

}