using gui_generator.multimappers.common_tools;
using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using System.Collections.Generic;

namespace gui_generator.multimappers.mappers.main.type_mappers.@class
{
    public class ClassDependencies
    {

        int depth = 0;
        IRecursiveMapperFactory mapperFactory;
        public ClassDependencies(IRecursiveMapperFactory mapperFactory, int depth)
        {
            this.depth = depth;
            this.mapperFactory = mapperFactory;
        }

        public CurrentValue[] Generate(ClassInstanceSpecification t)
        {

            var deps = new List<CurrentValue>();
            var constrParams = new ConstructorParamGetter().Get(t);
            foreach (var par in constrParams)
            {

                int deep = depth;
                if (par.Value.Instance == null) // increase depth if we are generating potential candidates and not real objects!
                    deep++;

                deps.Add(mapperFactory.Create(deep).ObjectToCurrentValue(par.Value, par.Key));
            }
            return deps.ToArray();
        }
    }

}
