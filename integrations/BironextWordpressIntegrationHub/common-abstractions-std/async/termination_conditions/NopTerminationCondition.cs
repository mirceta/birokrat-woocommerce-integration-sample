namespace biro_to_woo.logic.change_trackers.exhaustive
{
    public class NopTerminationCondition : ITerminationCondition {
        public bool ShouldStop() {
            return false;
        }

        public void Update() {
            
        }
    }
}
