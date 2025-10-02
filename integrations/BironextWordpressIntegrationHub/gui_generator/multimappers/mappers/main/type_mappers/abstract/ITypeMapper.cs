using System;

namespace gui_generator.multimappers.mappers.main.type_mappers.@abstract
{
    public interface ITypeMapper
    {
        CurrentValue Map(ClassInstanceSpecification os);
        string Map(CurrentValue val);
    }

    public class ClassInstanceSpecification
    {

        public Type InterfaceType { get; }
        public Type Type { get; }
        public object Instance { get; }

        public ClassInstanceSpecification(Type interfaceType, Type type, object obj)
        {

            if (interfaceType == null && type == null && obj == null) throw new Exception("All components cannot be null at the same time!");


            InterfaceType = interfaceType;
            Instance = obj;
            Type = type;
        }

    }
}
