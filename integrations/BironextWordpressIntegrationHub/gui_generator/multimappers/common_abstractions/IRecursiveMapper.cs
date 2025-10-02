using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using System.Linq;

namespace gui_generator
{
    public interface IRecursiveMapper {
        string CurrentValueToDefinition(CurrentValue val);
        CurrentValue ObjectToCurrentValue(ClassInstanceSpecification o, string variable);
    }

}
