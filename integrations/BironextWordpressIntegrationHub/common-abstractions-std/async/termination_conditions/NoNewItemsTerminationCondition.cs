using System.Collections.Concurrent;

namespace biro_to_woo.logic.change_trackers.exhaustive
{
    public class NoNewItemsTerminationCondition<T> : ITerminationCondition
    {
        ConcurrentBag<T> items;
        int lastCnt = 0;
        public NoNewItemsTerminationCondition(ConcurrentBag<T> items) {
            this.items = items;
        }

        public void Update() {
            lastCnt = items.Count;
        }
        public bool ShouldStop() {
            return items.Count == lastCnt;
        }
    }
}
