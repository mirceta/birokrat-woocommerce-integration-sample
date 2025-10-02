namespace biro_to_woo.logic.change_trackers.exhaustive
{
    public interface ITerminationCondition {
        void Update();
        bool ShouldStop();
    }
}
