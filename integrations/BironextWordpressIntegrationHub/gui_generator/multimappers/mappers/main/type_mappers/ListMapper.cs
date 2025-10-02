using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace gui_generator.multimappers.mappers.main.type_mappers
{

    public class ListMapper : TypeMapper
    {

        public ListMapper(IRecursiveMapperFactory mapperFactory, string variable, int depth = 0) : base(mapperFactory, variable, depth) { }

        public override CurrentValue Map(ClassInstanceSpecification o)
        {

            CurrentValue result = base.InitValue(o);
            result.type = o.Type.GetGenericArguments()[0].Name;
            // currently no template to parse T out of List<T>. We need this to create new elements! 
            if (o.Instance == null)
                return result;

            if (o.Type.GetGenericArguments()[0].IsEnum)
            {
                CaseOfEnumGeneric(o, result);
            }
            else if (o.Type.GetGenericArguments()[0].Name.Contains("KeyValuePair`2"))
            {
                CaseOfKeyvaluepairs(o, result);
            }
            else
            {
                var lst = ((IEnumerable<object>)o.Instance).Cast<object>().ToList();
                var els = new List<CurrentValue>();
                foreach (var val in lst)
                {
                    els.Add(mapperFactory.Create(depth).ObjectToCurrentValue(new ClassInstanceSpecification(null, val.GetType(), val), ""));
                }
                result.elements = els.ToArray();
            }
            return result;
        }

        private void CaseOfKeyvaluepairs(ClassInstanceSpecification o, CurrentValue result)
        {
            var lst = new List<KeyValuePair<object, object>>();
            var collection = o.Instance as IEnumerable;
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    var itemType = item.GetType();
                    if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    {
                        var key = itemType.GetProperty("Key").GetValue(item);
                        var value = itemType.GetProperty("Value").GetValue(item);
                        lst.Add(new KeyValuePair<object, object>(key, value));
                    }
                }
            }
            var els = new List<CurrentValue>();
            foreach (var val in lst)
            {
                els.Add(mapperFactory.Create(depth).ObjectToCurrentValue(new ClassInstanceSpecification(null, val.GetType(), val), ""));
            }
            result.elements = els.ToArray();
        }

        private void CaseOfEnumGeneric(ClassInstanceSpecification o, CurrentValue result)
        {
            var lst = (dynamic)o.Instance;
            var els = new List<CurrentValue>();
            foreach (var val in lst)
            {
                els.Add(mapperFactory.Create(depth).ObjectToCurrentValue(new ClassInstanceSpecification(null, val.GetType(), val), ""));
            }
            result.elements = els.ToArray();
        }

        public override string Map(CurrentValue val)
        {
            string some = $"new List<{val.type}>() {{\n";

            if (val.elements != null)
            {
                var tmp = val.elements.Select(x => mapperFactory.Create(depth).CurrentValueToDefinition(x) + "\n");
                some += string.Join(",", tmp);
            }
            some += "}\n";
            return Enumerable.Range(0, depth).Aggregate("", (x, y) => x + "\t") + some;
        }
    }
}
