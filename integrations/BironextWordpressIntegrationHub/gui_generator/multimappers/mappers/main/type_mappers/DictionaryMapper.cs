using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace gui_generator.multimappers.mappers.main.type_mappers
{
    public class DictionaryMapper : TypeMapper
    {

        public DictionaryMapper(IRecursiveMapperFactory mapperFactory, string variable, int depth = 0) : base(mapperFactory, variable, depth) { }

        public override CurrentValue Map(ClassInstanceSpecification os)
        {
            CurrentValue result = base.InitValue(os);
            result.type = os.Type.GetGenericArguments()[0].Name + "," + os.Type.GetGenericArguments()[1].Name;

            if (os.Instance == null)
                return result;

            var dic = ObjectToDictionaryHelper.ToDictionary(os.Instance);

            var keys = ((IEnumerable<object>)dic["Keys"]).Cast<string>().ToList();
            var values = ((IEnumerable<object>)dic["Values"]).Cast<object>().ToList();
            var lst = keys.Zip(values, (x, y) => new List<object>() { x, y }).ToList();

            var els = new List<CurrentValue>();
            foreach (var val in lst)
            {
                els.Add(mapperFactory.Create(depth).ObjectToCurrentValue(new ClassInstanceSpecification(null, val.GetType(), val), ""));
            }
            result.elements = els.ToArray();

            return result;
        }

        public override string Map(CurrentValue val)
        {
            string some = $"new Dictionary<{val.type}>() {{\n";

            var tmp = val.elements.Select(x =>
            {

                var key = x.elements[0].value;
                var value = x.elements[1];

                return $"{{ \"{key}\"," + mapperFactory.Create(depth).CurrentValueToDefinition(value) + " }\n";
            });

            some += string.Join(",", tmp);
            some += "}\n";
            return Enumerable.Range(0, depth).Aggregate("", (x, y) => x + "\t") + some;
        }
    }

    public static class ObjectToDictionaryHelper
    {
        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null)
                ThrowExceptionWhenSourceArgumentIsNull();

            var dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
                AddPropertyToDictionary(property, source, dictionary);
            return dictionary;
        }

        private static void AddPropertyToDictionary<T>(PropertyDescriptor property, object source, Dictionary<string, T> dictionary)
        {
            object value = property.GetValue(source);
            if (IsOfType<T>(value))
                dictionary.Add(property.Name, (T)value);
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        private static void ThrowExceptionWhenSourceArgumentIsNull()
        {
            throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is null.");
        }
    }
}
