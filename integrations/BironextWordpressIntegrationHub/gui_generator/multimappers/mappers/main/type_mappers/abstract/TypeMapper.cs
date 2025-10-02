namespace gui_generator.multimappers.mappers.main.type_mappers.@abstract
{
    public abstract class TypeMapper : ITypeMapper
    {
        protected int depth = 0;
        protected string variable = "";
        protected IRecursiveMapperFactory mapperFactory;

        public TypeMapper(IRecursiveMapperFactory mapperFactory, string variable, int depth = 0)
        {
            this.depth = depth;
            this.variable = variable;
            this.mapperFactory = mapperFactory;
        }

        public abstract CurrentValue Map(ClassInstanceSpecification os);
        public abstract string Map(CurrentValue val);

        protected CurrentValue InitValue(ClassInstanceSpecification os)
        {
            CurrentValue result = new CurrentValue();
            result.variable = variable;
            result.typeCategory = MappingHelper.GetJsonTypeCategory(os);

            if (os.InterfaceType == null && os.Type == null)
            {
                throw new System.Exception("Illegal state");
            }
            result.type = sig(os);

            return result;
        }

        private static string sig(ClassInstanceSpecification os)
        {
            string interf = os.InterfaceType?.Name ?? "";
            string cls = os.Type?.Name ?? "";
            return string.IsNullOrEmpty(interf) || string.IsNullOrEmpty(cls)
                ? interf + cls
                : $"{interf}.{cls}";
        }
    }
}
