using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace gui_generator.multimappers.mappers.main.type_mappers
{
    public class EnumMapper : TypeMapper
    {
        public EnumMapper(IRecursiveMapperFactory mapperFactory, string variable, int depth = 0) : base(mapperFactory, variable, depth)
        {
        }

        public override CurrentValue Map(ClassInstanceSpecification os)
        {

            CurrentValue result = base.InitValue(os);

            result.type = os.Type.FullName;
            if (os.Instance != null)
                result.value = Convert.ChangeType(os.Instance, os.Type).ToString();

            List<CurrentValue> elements = FillElements(os);
            result.elements = elements.ToArray();

            return result;
        }

        private static List<CurrentValue> FillElements(ClassInstanceSpecification os)
        {
            List<CurrentValue> elements = new List<CurrentValue>();
            var enumType = os.Type;
            var underType = Enum.GetUnderlyingType(enumType);
            var enumValues = Enum.GetValues(enumType);
            for (int i = 0; i < enumValues.Length; i++)
            {
                object eval = enumValues.GetValue(i); // SifraArtikla (BirokratField)
                object underVal = Convert.ChangeType(eval, underType); // 1 (int)
                elements.Add(new CurrentValue()
                {
                    value = eval.ToString()
                });
            }

            return elements;
        }

        public override string Map(CurrentValue val)
        {

            return Enumerable.Range(0, depth).Aggregate("", (x, y) => x + "\t") + val.type + "." + val.value;
            /*Assembly asm = Assembly.GetEntryAssembly();
            var enumType = Type.GetType(val.type);

            var underType = System.Enum.GetUnderlyingType(enumType);
            var enumValues = System.Enum.GetValues(enumType);
            for (int i = 0; i < enumValues.Length; i++) {
                object eval = enumValues.GetValue(i); // SifraArtikla (BirokratField)
                object underVal = System.Convert.ChangeType(eval, underType); // 1 (int)

                if (underVal.GetType() != typeof(int))
                    throw new Exception("Enum types other than integer are not supported!");
                if (eval.ToString() == val.value) {
                    return eval.ToString();
                }
            }
            throw new Exception("The value was not found in the relevant enum!");
            */
        }
    }
}
