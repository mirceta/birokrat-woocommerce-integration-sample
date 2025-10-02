using gui_attributes;
using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace gui_generator.multimappers.common_tools
{
    public class ConstructorParamGetter
    {
        public ConstructorParamGetter() { }

        public Dictionary<string, ClassInstanceSpecification> Get(ClassInstanceSpecification os)
        {

            if (os.Type == null)
                throw new Exception("Type is null. If type is null we cannot find the constructor!");

            ConstructorInfo ctor = GetConstructor(os.Type);
            var pars = ctor.GetParameters();
            return ParametersToClassInstanceSpecs(os, pars);
        }

        private static Dictionary<string, ClassInstanceSpecification> ParametersToClassInstanceSpecs(ClassInstanceSpecification os, ParameterInfo[] pars)
        {
            Dictionary<string, ClassInstanceSpecification> values = new Dictionary<string, ClassInstanceSpecification>();
            foreach (var par in pars)
            {
                ClassInstanceSpecification solution = new ConstructorParameterAdapter(par, os).Get();
                values[par.Name] = solution;
            }
            return values;
        }

        public static ConstructorInfo GetConstructor(Type some)
        {
            var constructors = some.GetConstructors();
            ConstructorInfo selectedConstructor = null;

            if (constructors.Length == 0)
            {
                throw new Exception($"{some.FullName} THERE MUST BE AT LEAST ONE CONSTRUCTOR IN THE CLASS");
            }
            else if (constructors.Length == 1)
            {
                selectedConstructor = constructors[0];
            }
            else
            {
                var myConstructors = constructors.Where(c => c.GetCustomAttributes(typeof(GuiConstructorAttribute), false).Length > 0).ToList();

                if (myConstructors.Count != 1)
                {
                    throw new Exception($"{some.FullName} THERE SHOULD BE EXACTLY ONE CONSTRUCTOR WITH THE GuiConstructor ATTRIBUTE IN THE CLASS");
                }

                selectedConstructor = myConstructors[0];
            }

            // Validate selected constructor
            if (!selectedConstructor.IsPublic || selectedConstructor.IsStatic)
            {
                throw new Exception($"{some.FullName} THE CONSTRUCTOR MUST BE PUBLIC AND CANNOT BE STATIC");
            }

            return selectedConstructor;
        }
    }
}
