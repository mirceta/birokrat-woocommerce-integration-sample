using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using System.Collections.Generic;

namespace gui_generator.multimappers.mappers
{

    public class VariableExtractorRecursiveMapper : IRecursiveMapper
    {

        List<string> desiredExtractedVariableTypes = new List<string>();
        IRecursiveMapper next;

        Dictionary<string, CurrentValue> singletons;
        Dictionary<string, bool> extractionInProgress;

        public VariableExtractorRecursiveMapper(
                List<string> typesToVariables,
                IRecursiveMapper next,
                Dictionary<string, CurrentValue> singletons,
                Dictionary<string, bool> extractionStarted,
                int depth = 0)
        {
            this.singletons = singletons;
            desiredExtractedVariableTypes = typesToVariables;
            this.extractionInProgress = extractionStarted;
            this.next = next;
        }
        public string CurrentValueToDefinition(CurrentValue val)
        {
            if (val.value != null && val.value.Contains("@@"))
                return val.value.Replace("@@", "").ToLower();
            return next.CurrentValueToDefinition(val);
        }
        public CurrentValue ObjectToCurrentValue(ClassInstanceSpecification o, string variable)
        {
            string currentType = null; // maybe think a bit more about this?
            if (o.InterfaceType != null)
                currentType = o.InterfaceType.Name;
            else
                currentType = o.Type.Name;

            if (!desiredExtractedVariableTypes.Contains(currentType))
                return next.ObjectToCurrentValue(o, variable);


            if (extractionInProgress.ContainsKey(currentType) && extractionInProgress[currentType] == true) {
                return next.ObjectToCurrentValue(o, variable);
            }

            return handleExtractedVariableCase(o, variable, currentType);
        }
        private CurrentValue handleExtractedVariableCase(ClassInstanceSpecification o, string variable, string currentType)
        {


            // Why do we need extractionStarted dictionary?
            // Sometimes for desiredExtractedVariableType A, it will happen that one of its decendents will
            // also be of type A. We must ensure that the hierarchically highest instance of A will be cached as
            // a variable - otherwise we will get a variable that will have a recursive reference to the same varible such as:
            // {variable: @@somevar, type: A, dependencies: { type: A, value: @@somevar } } - we don't want that here.
            // During extraction of variable, once we have started extracting it, then under it, no extraction can be done
            // of the same type!
            extractionInProgress[currentType] = true;

            if (!singletons.ContainsKey("@@" + currentType))
            {
                singletons["@@" + currentType] = next.ObjectToCurrentValue(o, variable);
            }

            // after we are done extracting, we need to default to using the variable reference again!!!
            extractionInProgress[currentType] = false;

            return new CurrentValue()
            {
                variable = variable,
                value = "@@" + currentType
            };
        }
    }

}
