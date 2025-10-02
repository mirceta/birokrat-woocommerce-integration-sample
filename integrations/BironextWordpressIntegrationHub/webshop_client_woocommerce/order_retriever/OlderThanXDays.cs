using biro_to_woo.logic.change_trackers.exhaustive;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace tests_fixture
{
    internal class OlderThanXDays : ITerminationCondition {
        
        ConcurrentBag<OrderDescription> items;  
        int days = 0;
        
        public OlderThanXDays(ConcurrentBag<OrderDescription> items, int days) {
             this.items = items;
            this.days = days;
         }

         public void Update() {
         }
         public bool ShouldStop() {
             return AreThereOrdersOlderThanXDays(days);
         }

         private bool AreThereOrdersOlderThanXDays(int days) {
            var anon = new { id = "", date = "" };
            if (items.Count == 0) return false;
            var sm = items
                    .Select(x => DateTime.ParseExact(x.date_created, "yyyy-MM-dd", CultureInfo.InvariantCulture))
                    .Any(x => x < DateTime.Now.Subtract(new TimeSpan(days, 0, 0, 0)));
            return sm;
         }
     }

    internal class MoreThanMaxCountOfItems : ITerminationCondition
    {
        ConcurrentBag<OrderDescription> items;
        int maxItemsCount;
        public MoreThanMaxCountOfItems(ConcurrentBag<OrderDescription> items, int maxItemsCount)
        {
            this.items = items;
            this.maxItemsCount = maxItemsCount;
        }
        public void Update()
        {
        }
        public bool ShouldStop()
        {
            if (items.Count > maxItemsCount)
                return true;
            return false;
        }
    }

    public class OrTerminationCondition : ITerminationCondition
    {
        List<ITerminationCondition> terminationConditions;
        public OrTerminationCondition(List<ITerminationCondition> terminationConditions) { 
            this.terminationConditions = terminationConditions;
        }

        public bool ShouldStop()
        {
            return terminationConditions.Any(x => x.ShouldStop());
        }

        public void Update()
        {
            terminationConditions.ForEach(x => x.Update());
        }
    }
}
