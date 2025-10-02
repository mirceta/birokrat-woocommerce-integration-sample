using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using gui_inferable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gui_generator.multimappers.mappers.main.type_mappers.@class
{


    public class WithInferableStateInterception : ITypeMapper
    {

        ClassMapper classMapper;
        Dictionary<string, object> inferredState;
        public WithInferableStateInterception(Dictionary<string, object> inferredState, ClassMapper mapper) {
            this.classMapper = mapper;
            this.inferredState = inferredState;
        }

        public CurrentValue Map(ClassInstanceSpecification t)
        {
            if (t.Instance != null)
            {
                if (t.Instance.GetType().GetInterfaces().Contains(typeof(IInferable)))
                {
                    inferredState = ((IInferable)t.Instance).Infer(inferredState);
                }
            }
            return classMapper.Map(t);
        }

        public string Map(CurrentValue val)
        {
            return classMapper.Map(val);
        }
    }

    public class ClassMapper : TypeMapper
    {

        public ClassMapper(IRecursiveMapperFactory mapperFactory, string variable, int depth = 0) : base(mapperFactory, variable, depth)
        {
        }

        public override CurrentValue Map(ClassInstanceSpecification t)
        {
            CurrentValue result = InitValue(t);

            if (t.Type != null && t.Type.Name == "BirokratObvezneNastavitve")
                Console.WriteLine();

            ThrowExceptionIfIllegalState(t);

            if (t.Type != null)
            {
                result.dependencies = new ClassDependencies(mapperFactory, depth).Generate(t);
            }

            if (t.InterfaceType != null)
            {
                // increase depth if we are generating potential candidates and not real objects!
                result.implementationOptions = new ImplementationOptions(mapperFactory, depth + 1).Generate(t, variable);
            }
            return result;
        }

        private static void ThrowExceptionIfIllegalState(ClassInstanceSpecification t)
        {
            string some = "";
            some += t.InterfaceType == null ? "X" : "O";
            some += t.Type == null ? "X" : "O";
            some += t.Instance == null ? "X" : "O";

            Dictionary<string, string> dic = new Dictionary<string, string>() {
                { "XOX", "class" },
                { "XOO", "classinstance" },
                { "OXX", "interface" },
                { "OOX", "interfaceimplementation" },
                { "OOO", "interfaceinstance" }
            };
            if (!dic.ContainsKey(some))
            {
                throw new Exception("Illegal state of class instance specification o!");
            }
        }

        public override string Map(CurrentValue val)
        {
            if (val.dependencies == null)
            {
                return Enumerable.Range(0, depth).Aggregate("", (x, y) => x + "\t") + "null";
            }

            var typePrefix = val.type.Contains(".")
                    ? val.type.Substring(val.type.IndexOf(".") + 1)
                    : val.type;
            string some = $"new {typePrefix}(\n";

            var tmp = val.dependencies.Select(x => mapperFactory.Create(depth).CurrentValueToDefinition(x));

            some += string.Join(",\n", tmp);
            some += ")";
            return Enumerable.Range(0, depth).Aggregate("", (x, y) => x + "\t") + some;

        }
    }

}
