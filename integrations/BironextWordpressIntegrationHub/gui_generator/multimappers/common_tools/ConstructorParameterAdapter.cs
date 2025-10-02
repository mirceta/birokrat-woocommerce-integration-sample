using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using System;
using System.Linq;
using System.Reflection;

namespace gui_generator.multimappers.common_tools
{

    public class ConstructorParameterAdapter
    {

        string name = "";
        ParameterInfo par;
        ClassInstanceSpecification caller;
        public ConstructorParameterAdapter(ParameterInfo par, ClassInstanceSpecification caller)
        {

            if (par == null)
                throw new ArgumentNullException("par");
            if (caller == null)
                throw new ArgumentNullException("caller");

            this.caller = caller;
            this.par = par;
            name = par.Name;
        }

        public ClassInstanceSpecification Get()
        {
            ClassInstanceSpecification solution = null;

            Type interfaceType = null;
            object value = null;
            Type type = null;
            if (!par.ParameterType.IsInterface && caller.Instance != null)
            {
                type = par.ParameterType;
                value = getParameterValueFromInstance();
            }
            else if (!par.ParameterType.IsInterface && caller.Instance == null)
            {
                type = par.ParameterType;
            }
            else if (par.ParameterType.IsInterface && caller.Instance != null)
            {
                interfaceType = par.ParameterType;
                value = getParameterValueFromInstance();
                type = value == null ? null : value.GetType();
            }
            else if (par.ParameterType.IsInterface && caller.Instance == null)
            {
                interfaceType = par.ParameterType;
            }
            if (interfaceType == null && value == null && type == null)
                throw new Exception($"All components are null - this should never happen!");
            solution = new ClassInstanceSpecification(interfaceType, type, value);
            return solution;
        }

        private object getParameterValueFromInstance()
        {
            object value;
            if (caller.Type == null)
                throw new Exception("Incorrect usage: type can never be null if instance is some");
            if (caller.Type != caller.Instance.GetType())
                throw new Exception($"Incorrect usage: os.Type and the type of os.Instance should be the same thing, but are different os.Type: {caller.Type.Name} os.Instance.GetType(): {caller.Instance.GetType()}");

            Type rootType = caller.Type;
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            FieldInfo field = rootType.GetField(name, bindFlags);
            TestConstructorParamBecomesMemberOfClass(caller.Instance, field);
            value = field.GetValue(caller.Instance);
            return value;
        }

        private void TestConstructorParamBecomesMemberOfClass(object o, FieldInfo field)
        {
            object sm = null;
            try
            {
                sm = field.GetValue(o);
            }
            catch (Exception ex)
            {
                throw new Exception($"{o.GetType().FullName} THE CLASS MUST HAVE PRIVATE VARIABLES WITH THE NAME AND TYPE EQUIVALENT TO THE CONSTRUCTOR PARAM NAMES!"
                        + $"***{par.Name}*** WAS NOT FOUND AS AN INSTANCE MEMBER!");
            }

            if (sm == null)
                return; // cannot compare types because one is null

            var partype = par.ParameterType;
            var fieldtype = sm.GetType();
            if (partype != fieldtype &&
                    !(fieldtype.IsClass && partype.IsInterface && fieldtype.GetInterfaces().Contains(partype))
                )
            {
                throw new Exception($"{o.GetType().FullName} THE CLASS MUST HAVE PRIVATE VARIABLES WITH THE NAME AND TYPE EQUIVALENT TO THE CONSTRUCTOR PARAM NAMES!"
                        + $"***{par.Name}*** IS OF DIFFERENT TYPE IN THE CONSTRUCTOR AND THE INSTANCE MEMBER!");
            }
        }
    }
}
