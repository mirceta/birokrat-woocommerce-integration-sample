using System;
using System.Threading.Tasks;

namespace common_multithreading
{
    /// <summary>
    /// Executes a non-generic <see cref="Task"/> with a specified timeout.
    /// </summary>
    public class TaskWithTimeout
    {
        private readonly int _timeout;
        private readonly Task _task;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskWithTimeout"/> class.
        /// </summary>
        /// <param name="task">The task to execute with a timeout constraint.</param>
        /// <param name="timeout">The timeout in milliseconds before the task is considered to have failed.</param>
        public TaskWithTimeout(Task task, int timeout)
        {
            _timeout = timeout;
            _task = task;
        }

        /// <summary>
        /// Executes the task and throws a <see cref="TimeoutException"/> if it does not complete in time.
        /// </summary>
        /// <returns>A task that completes when the underlying task finishes or a timeout occurs.</returns>
        /// <exception cref="TimeoutException">Thrown if the task does not complete within the timeout period.</exception>
        public async Task Run()
        {
            var completed = await Task.WhenAny(Task.Delay(_timeout), _task);

            if (completed != _task)
                throw new TimeoutException();
        }
    }

    /// <summary>
    /// Executes a generic <see cref="Task{TResult}"/> with a specified timeout.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the task.</typeparam>
    public class TaskWithTimeout<T>
    {
        private readonly int _timeout;
        private readonly Task<T> _task;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskWithTimeout{T}"/> class.
        /// </summary>
        /// <param name="task">The task to execute with a timeout constraint.</param>
        /// <param name="timeout">The timeout in milliseconds before the task is considered to have failed.</param>
        public TaskWithTimeout(Task<T> task, int timeout)
        {
            _timeout = timeout;
            _task = task;
        }

        /// <summary>
        /// Executes the task and returns the result if it completes in time; otherwise, throws a <see cref="TimeoutException"/>.
        /// </summary>
        /// <returns>The result of the task if completed within the timeout.</returns>
        /// <exception cref="TimeoutException">Thrown if the task does not complete within the timeout period.</exception>
        public async Task<T> Run()
        {
            var completed = await Task.WhenAny(Task.Delay(_timeout), _task);

            if (completed == _task)
                return _task.Result;

            throw new TimeoutException();
        }
    }
}
