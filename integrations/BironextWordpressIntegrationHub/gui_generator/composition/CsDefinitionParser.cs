using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using core;

namespace gui_generator.cs_definition_serializer
{

    public interface IMock<T>
    {
        public T Get();
    }

    public class CsDefinitionParser<T>
    {

        RuntimeCompilation compiler;
        string usingStatements;
        string classNamespace;
        IMockTemplateGenerator templateGenerator;


        public CsDefinitionParser(RuntimeCompilation compiler,
            string usingStatements,
            string mockInterfaceNamespace,
            IMockTemplateGenerator templateGenerator = null)
        {
            this.compiler = compiler;
            this.usingStatements = usingStatements;
            classNamespace = mockInterfaceNamespace;

            if (templateGenerator == null)
                this.templateGenerator = new MockTemplateGenerator<T>(usingStatements, mockInterfaceNamespace);
            else
                this.templateGenerator = templateGenerator;

        }

        public core.zgeneric.IMockWithInject<T> Create(Dictionary<string, string> neededVariablesCsDefs, string csdefinition)
        {
            string content = templateGenerator.Generate(neededVariablesCsDefs, csdefinition);
            string path = compiler.CreateAssembly_Then_ReturnPath(content);
            var asm = Assembly.LoadFile(path);
            Type type = asm.GetType($"core.Mock");
            core.zgeneric.IMockWithInject<T> mock = (core.zgeneric.IMockWithInject<T>)Activator.CreateInstance(type);
            return mock;
        }
    }


}
