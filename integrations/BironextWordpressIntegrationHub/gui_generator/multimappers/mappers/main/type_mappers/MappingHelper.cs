using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using System;
using System.Collections.Generic;

namespace gui_generator.multimappers.mappers.main.type_mappers
{
    public class MappingHelper
    {
        public static string GetJsonTypeCategory(ClassInstanceSpecification o)
        {
            if (o.Type != null && o.Type.IsInterface)
            {
                throw new Exception("o.Type cannot be interface!");
            }
            if (o.Type != null && !IsClass(o.Type))
            {
                return ParseSimple(o);
            }
            return ParseComplex(o);
        }

        private static string ParseComplex(ClassInstanceSpecification o)
        {
            string some = "";
            some += o.InterfaceType == null ? "X" : "O";
            some += o.Type == null ? "X" : "O";
            some += o.Instance == null ? "X" : "O";

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
            return "class";
        }

        private static string ParseSimple(ClassInstanceSpecification o)
        {
            if (o.Type.Name.Contains("Dictionary"))
            {
                return "dictionary";
            }
            else if (o.Type.Name.Contains("List`1"))
            {
                return "list";
            }
            else if (o.Type.IsEnum)
            {
                return "enum";
            }
            else if (o.Type.IsPrimitive || o.Type.Name.ToLower() == "string")
            {
                return "primitive";
            }
            else
            {
                throw new Exception("TYPE NOT SUPPORTED!");
            }
        }

        private static bool IsClass(Type t)
        {
            if (t.Name.Contains("Dictionary"))
            {
                return false;
            }
            else if (t.Name.Contains("List`1"))
            {
                return false;
            }
            else if (t.IsEnum)
            {
                return false;
            }
            else if (t.IsPrimitive || t.Name.ToLower() == "string")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
