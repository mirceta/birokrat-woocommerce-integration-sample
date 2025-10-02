using System.Collections.Generic;
using System.Linq;

namespace gui_generator.cs_definition_serializer
{
    public class LazyIntegrationTemplateGenerator : IMockTemplateGenerator
    {
        private string usingStatements;
        private string classNamespace;
        private string integrationName;
        private string integrationType;

        public LazyIntegrationTemplateGenerator(string usingStatements, string classNamespace, string integrationName, string integrationType)
        {
            this.usingStatements = usingStatements;
            this.classNamespace = classNamespace;
            this.integrationName = integrationName;
            this.integrationType = integrationType;
        }

        public string Generate(Dictionary<string, string> neededVariablesCsDefs, string csObjectDefinition)
        {
            

            string varsDeclarations = string.Join(";\n",
                neededVariablesCsDefs.ToList().Select(x => $"{x.Key} = null"));

            string varsAssignments = string.Join(";\n",
                neededVariablesCsDefs.ToList().Select(x => $"{x.Key.Split(" ")[1]} = {x.Value}"));



            string template = @$"
                {usingStatements}
                using System.Reflection;

                namespace {classNamespace} {{
                    public class Mock : IMockWithInject<LazyIntegration> {{

                        {varsDeclarations};

                        public Mock() {{
                            {varsAssignments};
                        }}

                        public void Inject(Dictionary<string, object> injections) {{
                            FieldInfo[] fieldInfos = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            foreach (var x in injections.Keys) {{
                                foreach (var fieldInfo in fieldInfos)
                                {{
                                    if (x == fieldInfo.Name) {{
                                        fieldInfo.SetValue(this, injections[x]);
                                    }}
                                }}
                            }}
                        }}

                        public void SetFieldProperty(string fieldName, string propertyName, object value)
                        {{
                            // Get the FieldInfo of the field we want to access.
                            FieldInfo fieldInfo = this.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (fieldInfo == null)
                            {{
                                throw new ArgumentException($""No field named {{fieldName}} found"", nameof(fieldName));
                            }}

                            // Get the current value of the field. We expect it to be an object.
                            object fieldValue = fieldInfo.GetValue(this);
                            if (fieldValue == null)
                            {{
                                throw new NullReferenceException($""Field {{fieldName}} is null"");
                            }}

                            // Get the PropertyInfo of the property we want to set.
                            PropertyInfo propertyInfo = fieldValue.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (propertyInfo == null)
                            {{
                                throw new ArgumentException($""No property named {{propertyName}} found in {{fieldName}}"", nameof(propertyName));
                            }}

                            // Set the value of the property.
                            propertyInfo.SetValue(fieldValue, value);
                        }}

                        public Dictionary<string, object> GetFields()
                        {{
                            Dictionary<string, object> fieldsDictionary = new Dictionary<string, object>();
                            FieldInfo[] fieldInfos = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

                            foreach (var fieldInfo in fieldInfos)
                            {{
                                // Include a check here if you want to exclude certain fields
                                // For instance, if you don't want to include fields of certain types, you could do so.
        
                                // Retrieve the value of the field for 'this' instance
                                object fieldValue = fieldInfo.GetValue(this);
        
                                // Add the field name and value to the dictionary
                                fieldsDictionary.Add(fieldInfo.Name, fieldValue);
                            }}

                            return fieldsDictionary;
                        }}

                        public LazyIntegration Get() {{
                            return new LazyIntegration()
                                    {{
                                        Name = ""{integrationName}"",
                                        Type = ""{integrationType}"",
                                        Key = """",
                                        Integration = () => {{
                                            return {csObjectDefinition};
                                        }}
                                    }};
                        }}

                        public string GetSignature() {{
                            return ""{integrationType}:{integrationName}"";
                        }}
                    }}
                }}
                ";

            return template;
        }
    }


}
