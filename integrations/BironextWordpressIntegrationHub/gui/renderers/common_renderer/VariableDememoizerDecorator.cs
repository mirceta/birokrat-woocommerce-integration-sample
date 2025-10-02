using gui_generator;
using System.Linq;

namespace gui_gen
{
    public class VariableDememoizerDecorator : IDememoizer
    {
        private readonly IDememoizer _innerDememoizer;
        private readonly CurrentValue[] _variables;

        public VariableDememoizerDecorator(IDememoizer innerDememoizer, CurrentValue[] variables)
        {
            _innerDememoizer = innerDememoizer;
            _variables = variables;
        }

        public CurrentValue Find(string type)
        {
            return _innerDememoizer.Find(type);
        }

        public CurrentValue Expand(CurrentValue value)
        {
            // First apply variable substitution, then expand
            var substitutedValue = ReplaceValuesInDepsAndImplOptsIfMatch(value);
            return _innerDememoizer.Expand(substitutedValue);
        }

        public CurrentValue Explode(CurrentValue value)
        {
            // First apply variable substitution, then explode
            var substitutedValue = ReplaceValuesInDepsAndImplOptsIfMatch(value);
            return _innerDememoizer.Explode(substitutedValue);
        }

        private CurrentValue ReplaceValuesInDepsAndImplOptsIfMatch(CurrentValue currentValue)
        {
            if (currentValue.dependencies != null)
            {
                currentValue.dependencies = currentValue.dependencies.Select(dep => ReplaceValue(dep)).ToArray();
            }
            if (currentValue.implementationOptions != null)
            {
                currentValue.implementationOptions = currentValue.implementationOptions.Select(implOpt => ReplaceValue(implOpt)).ToArray();
            }
            return currentValue;
        }

        private CurrentValue ReplaceValue(CurrentValue currentValue)
        {
            if (currentValue.variable != null && currentValue.variable.StartsWith("@@"))
            {
                // Already a variable, no substitution needed
                return currentValue;
            }

            var matchingVar = _variables.Where(element =>
            { 
                if (string.IsNullOrEmpty(currentValue.type)) return false;
                return element.type.Contains(currentValue.type.Replace("@#@#", ""));
            }).ToList(); ;


            if (matchingVar.Count() == 1 && matchingVar.Single().variable.Contains("@@")) {
                currentValue.value = matchingVar.Single().variable;

                // see below - we will set typeCategory to "variable"! we set this because if type category
                // in "variable" during rendering, 
                // then render the HiddenRender element - which means that this component will be invisible
                // in the final render = this is what we want for variables => they should only be changed on
                // exactly one place!!
                currentValue.typeCategory = "variable";
            }
            return currentValue;
        }
    }




}
