using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace gui_generator.multimappers.mappers.main.type_mappers
{
    public class PrimitiveMapper : TypeMapper
    {
        public PrimitiveMapper(IRecursiveMapperFactory mapperFactory, string variable, int depth = 0) : base(mapperFactory, variable, depth)
        {
        }

        public override CurrentValue Map(ClassInstanceSpecification os)
        {
            CurrentValue result = base.InitValue(os);
            result.type = os.Type.Name;
            if (os.Instance != null)
                result.value = os.Instance.ToString(); // need to parse booleanetc!!!!
            return result;
        }

        public override string Map(CurrentValue val)
        {



            string result = "";
            if (val == null)
            {
                result = "null";
            }
            else if (val.type.ToLower() == "string")
            {
                result = "@\"" + val.value + "\"";
            }
            else if (val.type.ToLower().Contains("int"))
            {
                int some = int.Parse(val.value);
                result = some.ToString();
            }
            else if (val.type.ToLower() == "boolean")
            {
                if (val.value.ToLower() == "true")
                {
                    result = "true";
                }
                else if (val.value.ToLower() == "false")
                {
                    result = "false";
                }
                else
                {
                    throw new Exception("Boolean value was something else than true or false: " + val.value);
                }
            }
            else if (val.type.ToLower() == "double") { 
                double sm = double.Parse(val.value);
                result = sm.ToString().Replace(",", ".");
            }
            else
            {
                throw new Exception("Unsupported primitive type");
            }
            result = Enumerable.Range(0, depth).Aggregate("", (x, y) => x + "\t") + result;
            return result;
        }
    }
}
