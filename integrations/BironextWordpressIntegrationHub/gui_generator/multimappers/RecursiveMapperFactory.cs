using gui_generator.multimappers.mappers;
using gui_generator.multimappers.mappers.main;
using System.Collections.Generic;

namespace gui_generator.multimappers.injector_addon
{
    public class RecursiveMapperFactory : IRecursiveMapperFactory
    {

        List<string> desiredVariableTypes;
        Dictionary<string, CurrentValue> singletons;
        Dictionary<string, bool> variableExtractionInProgress;
        Dictionary<string, CurrentValue> dicMemoizedTypes;
        Dictionary<string, object> inferredState;

        public RecursiveMapperFactory(List<string> desiredVariableTypes, 
            Dictionary<string, CurrentValue> singletons,
            Dictionary<string, CurrentValue> dicMemoizedTypes,
            Dictionary<string, object> inferredState)
        {
            this.desiredVariableTypes = desiredVariableTypes;
            this.singletons = singletons;
            this.dicMemoizedTypes = dicMemoizedTypes;
            this.inferredState = inferredState;
            this.variableExtractionInProgress = new Dictionary<string, bool>();
        }

        public IRecursiveMapper Create(int depth) 
        {
            return new TypeMemoizationRecursiveMapper(dicMemoizedTypes, 
                        new VariableExtractorRecursiveMapper(desiredVariableTypes,
                            new MainRecursiveMapper(this, inferredState, depth),
                                singletons, 
                                variableExtractionInProgress,
                                depth));
        }

    }
}
