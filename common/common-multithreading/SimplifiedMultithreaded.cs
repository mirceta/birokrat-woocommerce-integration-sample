using common_multithreading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_multithreading_tests
{

    public class SimplifiedMultithreaded
    {
        private readonly List<Func<Task>> tasks;
        private readonly SemaphoreQueue fifoSemaphore;
        private const int MaxConcurrentTasks = 6;
        private readonly bool loopTaskForever;
        private readonly Action<int, string> updateTaskStatus;

        public SimplifiedMultithreaded(List<Func<Task>> tasks, Action<int, string> updateTaskStatus, bool loopTaskForever = false)
        {
            this.loopTaskForever = loopTaskForever;
            this.tasks = tasks;
            this.updateTaskStatus = updateTaskStatus;
            fifoSemaphore = new SemaphoreQueue(MaxConcurrentTasks, MaxConcurrentTasks);
        }

        public async Task Run()
        {
            List<Task> taskList = new List<Task>();
            for (int i = 0; i < tasks.Count; i++)
            {
                int taskIndex = i;
                Task t = new Task(async () =>
                {
                    if (!loopTaskForever)
                    {
                        await executeTask(tasks[taskIndex], taskIndex);
                    }
                    else
                    {
                        while (true)
                        {
                            await executeTask(tasks[taskIndex], taskIndex);
                        }
                    }
                });
                taskList.Add(t);
            }

            foreach (Task t in taskList)
            {
                t.Start();
            }

            await Task.WhenAll(taskList);
        }

        private async Task executeTask(Func<Task> taskFunc, int taskIndex)
        {
            await waitAtSemaphore(taskIndex);
            try
            {
                await taskFunc();
            }
            finally
            {
                fifoSemaphore.Release();
                updateTaskStatus(taskIndex, "completed");
            }
        }

        private async Task waitAtSemaphore(int taskIndex)
        {
            using (var timer = new System.Timers.Timer(1000))
            {
                timer.Elapsed += (sender, e) =>
                {
                    updateTaskStatus(taskIndex, "waiting");
                };
                timer.Start();

                await fifoSemaphore.WaitAsync();
                timer.Stop();

                updateTaskStatus(taskIndex, "entering");
            }
        }
    }





}
