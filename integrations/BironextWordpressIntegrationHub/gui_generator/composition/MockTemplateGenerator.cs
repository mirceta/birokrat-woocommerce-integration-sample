using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace gui_generator.cs_definition_serializer
{

    public interface IMockTemplateGenerator
    {
        string Generate(Dictionary<string, string> neededVariablesCsDefs, string csObjectDefinition);
    }

    public class MockTemplateGenerator<T> : IMockTemplateGenerator
    {
        private string usingStatements;
        private string classNamespace;

        public MockTemplateGenerator(string usingStatements, string classNamespace)
        {
            this.usingStatements = usingStatements;
            this.classNamespace = classNamespace;
        }

        public string Generate(Dictionary<string, string> neededVariablesCsDefs, string csObjectDefinition)
        {
            string vars = string.Join(";\n",
                neededVariablesCsDefs.ToList().Select(x => $"{x.Key} = {x.Value}"));

            string template = @$"
        {usingStatements}
        namespace {classNamespace} {{
            public class Mock : IMock<{typeof(T).FullName}> {{
                public {typeof(T).FullName} Get() {{
                    {vars};
                    return {csObjectDefinition};
                }}
            }}
        }}
        ";

            return template;
        }
    }


}
