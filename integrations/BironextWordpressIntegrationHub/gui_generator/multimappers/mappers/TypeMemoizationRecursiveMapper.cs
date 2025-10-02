using gui_generator.multimappers.common_tools;
using gui_generator.multimappers.mappers.main.type_mappers;
using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using gui_generator.multimappers.mappers.main.type_mappers.@class;
using RichardSzalay.MockHttp.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gui_generator.multimappers.mappers
{
    public class TypeMemoizationRecursiveMapper : IRecursiveMapper
    {

        Dictionary<string, CurrentValue> memo;
        IRecursiveMapper next;

        public TypeMemoizationRecursiveMapper(Dictionary<string, CurrentValue> memo,
            IRecursiveMapper next)
        {
            this.memo = memo;
            this.next = next;
        }

        public string CurrentValueToDefinition(CurrentValue val)
        {
            return next.CurrentValueToDefinition(val);
        }

        public CurrentValue ObjectToCurrentValue(ClassInstanceSpecification o, string variable)
        {
            if (o.Instance == null && // definitely an implementation option
                MappingHelper.GetJsonTypeCategory(o) == "class" && // is a class type
                    !(o.InterfaceType != null && o.Type == null)) // not an interface
            {
                return handleClassMemoization(o, variable);
            }
            else if (MappingHelper.GetJsonTypeCategory(o) == "list")
            {
                var type = o.Type.GetGenericArguments()[0];
                memoizeTypeForAddingElements(type, variable);
            }
            else if (MappingHelper.GetJsonTypeCategory(o) == "dictionary")
            {
                memoizeTypeForAddingElements(o.Type.GetGenericArguments()[0], variable);
                memoizeTypeForAddingElements(o.Type.GetGenericArguments()[1], variable);
            }
            else if (MappingHelper.GetJsonTypeCategory(o) == "enum") {

                var curvalenum = next.ObjectToCurrentValue(o, variable);

                if (curvalenum.elements != null && curvalenum.elements.Length > 0)
                {
                    memo[sig(o)] = new CurrentValue()
                    {
                        type = sig(o),
                        elements = curvalenum.elements,
                        typeCategory = "enum"
                    };

                    return curvalenum;
                }
                else {
                    return new CurrentValue { variable = variable, type = sig(o), typeCategory = "enum" };
                }
                
            }
            return next.ObjectToCurrentValue(o, variable);
        }

        private void memoizeTypeForAddingElements(Type type, string variable)
        {
            /*
            In this case the purpose is to inject the types of lists and dictionaries in order to
            add them to the memos array, but we don't actually change the list elements.
            We want to store them for when we want to add elements.
             */




            var listtypes = new List<ClassInstanceSpecification>();
            if (type.IsInterface)
            {
                var implOpts = ImplementationOptions.GetImplementationTypes(type);
                listtypes.AddRange(implOpts.Select(x => new ClassInstanceSpecification(type, x, null)).ToList());
            }
            else
            {
                listtypes.Add(new ClassInstanceSpecification(null, type, null));
            }

            foreach (var listtype in listtypes)
            {
                if (type.Name != "Object" && MappingHelper.GetJsonTypeCategory(listtype) == "class")
                {
                    handleClassMemoization(listtype, variable);
                }
            }
        }

        private CurrentValue handleClassMemoization(ClassInstanceSpecification o, string variable)
        {
            // if this class is already memoized, we don't expand on.
            if (memo.ContainsKey(sig(o)))
            {
                return new CurrentValue { variable = variable, type = sig(o), typeCategory = "class" };
            }

            // if not, first memoize it to avoid infinite loops of type:
            // A1(C,y,A) -> ... A1,A2,A3..... A1(C,y,A)... 
            var deps = makeMemo(o, next);
            memo[sig(o)] = new CurrentValue()
            {
                type = sig(o),
                dependencies = deps.ToArray(),
                typeCategory = "class"
            };
            next.ObjectToCurrentValue(o, variable);
            return new CurrentValue { variable = variable, type = sig(o), typeCategory = "class" };
        }

        List<CurrentValue> makeMemo(ClassInstanceSpecification o, IRecursiveMapper next) {
            var deps = new List<CurrentValue>();
            var constrParams = new ConstructorParamGetter().Get(o);
            foreach (var par in constrParams)
            {

                string typeCategory = MappingHelper.GetJsonTypeCategory(par.Value);
                if (typeCategory == "class")
                {
                    // just add a non existing memo for now, it will be expanded elsewhere!
                    deps.Add(new CurrentValue() { variable = par.Key, type = sig(par.Value), typeCategory = "class" });
                }
                else if (typeCategory == "dictionary")
                {
                    var keyType = new ClassInstanceSpecification(null, par.Value.Type.GetGenericArguments()[0], null);
                    var valueType = new ClassInstanceSpecification(null, par.Value.Type.GetGenericArguments()[1], null);
                    deps.Add(new CurrentValue()
                    {
                        variable = par.Key,
                        type = $"{sig(keyType)},{sig(valueType)}",
                        typeCategory = "dictionary"
                    });
                }
                else if (typeCategory == "list") {
                    var type = new ClassInstanceSpecification(null, par.Value.Type.GetGenericArguments()[0], null);
                    deps.Add(new CurrentValue()
                    {
                        variable = par.Key,
                        type = $"{sig(type)}",
                        typeCategory = "list"
                    });
                }
                else
                {
                    // what is this case anyway?
                    deps.Add(new CurrentValue() { variable = par.Key, typeCategory = typeCategory, type = par.Value.Type.Name });
                }
            }
            return deps;
        }

        private static string sig(ClassInstanceSpecification os)
        {
            string interf = os.InterfaceType?.Name ?? "";
            string cls = os.Type?.Name ?? "";
            string done = string.IsNullOrEmpty(interf) || string.IsNullOrEmpty(cls)
                ? interf + cls
                : $"{interf}.{cls}";
            return $"@#@#{done}";
        }
    }

}
