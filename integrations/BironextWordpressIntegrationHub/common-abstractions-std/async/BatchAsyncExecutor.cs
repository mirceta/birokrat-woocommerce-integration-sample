using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace biro_to_woo.logic.change_trackers.exhaustive {
    public class BatchAsyncExecutor {

        int batchCount;
        ITerminationCondition terminationCondition;

        public BatchAsyncExecutor(int batchCount, ITerminationCondition term) {
            this.batchCount = batchCount;
            this.terminationCondition = term;
        }

        public void Execute(List<Task> tasks) {
            List<List<Task>> batches = Partition(tasks.ToArray(), batchCount).ToList();
            foreach (var batch in batches) {
                terminationCondition.Update();
                foreach (Task t in batch) {
                    t.Start();
                }
                foreach (Task t in batch) {
                    t.Wait();
                }
                foreach (Task t in batch) {
                    t.Dispose();
                }

                if (terminationCondition.ShouldStop())
                    break;
            }
        }
        private IEnumerable<List<Task>> Partition(IList<Task> source, Int32 size) {
            for (int i = 0; i < Math.Ceiling(source.Count / (Double)size); i++)
                yield return new List<Task>(source.Skip(size * i).Take(size));
        }
    }
}
