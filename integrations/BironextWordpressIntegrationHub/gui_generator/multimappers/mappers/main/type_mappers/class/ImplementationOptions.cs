using gui_generator.multimappers.mappers.main.type_mappers.@abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gui_generator.multimappers.mappers.main.type_mappers.@class
{
    public class ImplementationOptions
    {

        int depth = 0;
        IRecursiveMapperFactory mapperFactory;
        public ImplementationOptions(IRecursiveMapperFactory mapperFactory, int depth)
        {
            this.depth = depth;
            this.mapperFactory = mapperFactory;
        }

        public CurrentValue[] Generate(ClassInstanceSpecification t, string variable)
        {
            List<Type> implementationTypes = null;
            if (t.InterfaceType != null)
            {
                implementationTypes = GetImplementationTypes(t.InterfaceType);
            }
            else if (t.Type != null)
            {
                implementationTypes = new List<Type> { t.Type };
            }
            else
            {
                throw new Exception("On or both of interface type or object type must always be some");
            }

            var options = new List<CurrentValue>();
            foreach (var impl in implementationTypes)
            {
                options.Add(mapperFactory.Create(depth)
                                .ObjectToCurrentValue(new ClassInstanceSpecification(t.InterfaceType, impl, null), variable));
            }
            return options.ToArray();
        }

        public static List<Type> GetImplementationTypes(Type interfaceType)
        {
            var intrf = interfaceType;
            var impls = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => intrf.IsAssignableFrom(p))
                .Where(x => x.Name != intrf.Name)
                .ToList();
            return impls;
        }
    }

}
