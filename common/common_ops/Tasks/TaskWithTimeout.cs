using System;
using System.Threading.Tasks;

namespace common_ops.Tasks
{

    public class TaskWithTimeout
    {
        private readonly int _timeout;
        private readonly Task _task;

        public TaskWithTimeout(int timeout, Task task)
        {
            _timeout = timeout;
            _task = task;
        }

        public async Task Run()
        {
            var completed = await Task.WhenAny(Task.Delay(_timeout), _task);

            if (completed != _task)
                throw new TimeoutException();
        }
    }

    public class TaskWithTimeout<T>
    {
        private readonly int _timeout;
        private readonly Task<T> _task;

        public TaskWithTimeout(int timeout, Task<T> task)
        {
            _timeout = timeout;
            _task = task;
        }

        public async Task<T> Run()
        {
            var completed = await Task.WhenAny(Task.Delay(_timeout), _task);

            if (completed == _task)
                return _task.Result;

            throw new TimeoutException();
        }
    }
}
