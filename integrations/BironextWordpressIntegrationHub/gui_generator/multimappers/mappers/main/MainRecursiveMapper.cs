using System;
using System.Collections.Generic;
using gui_generator.multimappers.mappers.main.type_mappers;
using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using gui_generator.multimappers.mappers.main.type_mappers.@class;

namespace gui_generator.multimappers.mappers.main
{
    public class MainRecursiveMapper : IRecursiveMapper
    {

        int depth = 0;
        const int MAXDEPTH = 10;
        IRecursiveMapperFactory mapperFactory;
        Dictionary<string, object> inferredState;

        public MainRecursiveMapper(IRecursiveMapperFactory mapperFactory, 
            Dictionary<string, object> inferredState,
            int depth = 0)
        {
            this.depth = depth; // WE KEEP DEPTH TO DEFEND AGAINST RECURIVE INTERFACES
            this.mapperFactory = mapperFactory;
            this.inferredState = inferredState;
            /*
             EXAMPLE: class SomeOrderCR: IOrderCr { SomeOrderCR(IOrderCR) }
                    When you will insert SomeOrderCR, it will get the interface and find all possible impls,
                            but the impl will contain a constructor with IOrderCR, where you will list possibilities
                            again, and you will loop forever.
             */
        }

        public CurrentValue ObjectToCurrentValue(ClassInstanceSpecification o, string variable)
        {

            if (depth >= MAXDEPTH)
            {
                return new CurrentValue();
            }
            if (o == null)
            {
                var t = new CurrentValue();
                t.variable = variable;
                return t; // WRONG -> THIS WAS WE CANNOT CHANGE NULL TO SOMETHING ELSE IF WE WANT TO!!!
            }
            string type = MappingHelper.GetJsonTypeCategory(o);
            return Work(type, variable).Map(o);
        }

        public string CurrentValueToDefinition(CurrentValue val)
        {

            if (depth >= MAXDEPTH)
            {
                throw new Exception("THIS SHOULD NOT HAPPEN");
            }
            if (val == null)
            {
                throw new Exception("THIS SHOULD NOT HAPPEN");
            }
            return Work(val.typeCategory, "").Map(val);
        }

        private ITypeMapper Work(string type, string variable)
        {
            //depth += 1;
            Dictionary<string, ITypeMapper> dictionaries = new Dictionary<string, ITypeMapper>() {
                { "primitive", new PrimitiveMapper(mapperFactory, variable, depth)},
                { "list", new ListMapper(mapperFactory, variable, depth)},
                { "dictionary", new DictionaryMapper(mapperFactory, variable, depth)},
                { "enum", new EnumMapper(mapperFactory, variable, depth)},
                { "class", 
                    new WithInferableStateInterception(inferredState,
                        new ClassMapper(mapperFactory, variable, depth))
                },
            };
            return dictionaries[type];
        }
    }

}
