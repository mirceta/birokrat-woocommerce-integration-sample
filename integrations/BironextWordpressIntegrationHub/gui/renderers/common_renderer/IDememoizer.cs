using gui_generator;

namespace gui_gen
{
    public interface IDememoizer
    {
        CurrentValue Expand(CurrentValue value);
        CurrentValue Explode(CurrentValue value);
        CurrentValue Find(string type);
    }
}
