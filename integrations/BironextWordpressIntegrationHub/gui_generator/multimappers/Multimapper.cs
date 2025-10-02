
using gui_generator.multimappers.injector_addon;
using gui_generator.multimappers.mappers;
using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using System.Collections.Generic;
using System.Linq;

namespace gui_generator
{
    public class Multimapper<T> {


        IRecursiveMapperFactory factory;
        Dictionary<string, CurrentValue> extractedVariables;
        Dictionary<string, CurrentValue> dicMemoizedTypes;
        Dictionary<string, object> inferredState;
        public Multimapper(List<string> desiredExtractedVariableTypes) {
            extractedVariables = new Dictionary<string, CurrentValue>();
            dicMemoizedTypes = new Dictionary<string, CurrentValue>();
            inferredState = new Dictionary<string, object>();

            factory = new RecursiveMapperFactory(
                desiredExtractedVariableTypes,  
                extractedVariables,
                dicMemoizedTypes,
                inferredState);
        }

        public string ToCsDefinitionString(CurrentValue value) {
            return factory.Create(0).CurrentValueToDefinition(value);
        }

        public CurrentValue ToCurrentValue(T obj) {
            var entry = new EntryObject<T>(obj);
            var o = new ClassInstanceSpecification(null, entry.GetType(), entry);

            var mapper = (TypeMemoizationRecursiveMapper)factory.Create(0);

            var root = mapper.ObjectToCurrentValue(o, "");
            AddExtractedVariables_To_RootDependencies(root);
            AddTypeMemos_To_RootImplementationOptions(root);
            return root;
        }

        public Dictionary<string, object> GetInferredState() {
            return inferredState;
        }

        private void AddExtractedVariables_To_RootDependencies(CurrentValue root) {

            var variables = extractedVariables.ToList().Select(x => {
                x.Value.variable = x.Key;
                return x.Value;
            }).ToList();

            variables.AddRange(root.dependencies);
            root.dependencies = variables.ToArray();
        }

        private void AddTypeMemos_To_RootImplementationOptions(CurrentValue root) {
            var variables = dicMemoizedTypes.ToList().Select(x => {
                return x.Value;
            }).ToList();

            root.implementationOptions = variables.ToArray();
        }
    }

}
