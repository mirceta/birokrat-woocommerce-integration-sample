using BiroWooHub.logic.integration;
using core.customers;
using Newtonsoft.Json;
using si.birokrat.next.common.build;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using core.zgeneric;
using gui_generator.cs_definition_serializer;
using core.structs;
using gui_generator_integs;

namespace gui_generator.api
{

    public class TestingConfigParser
    {
        private static readonly string[] ReferenceArray = { "on-hold", "processing", "completed" };



        public TestingConfiguration GetTestingConfig(IIntegration integration, List<string> inputArray)
        {
            var result =  inputArray.OrderBy(item => Array.IndexOf(ReferenceArray, item)).ToArray();

            return TestingConfigGenHelper.GetTestingConfiguration(integration, "DEFAULT", result.ToList());
        }
    }



    public class LazyIntegrationAdapter
    {

        public IMockWithInject<LazyIntegration> Adapt(CurrentValue curval, string integrationType)
        {

            var typemapper = new Multimapper<IIntegration>(StaticVariables.desiredExtractedVariableTypes);


            var deps = curval.dependencies.TakeWhile(x => x.type != "IIntegration.RegularIntegration");
            var main = curval.dependencies.TakeLast(1).Single();

            string integrationName = main.dependencies.Where(x => x.variable == "name").Single().value;


            var defDeps = deps.Select(x =>
            {
                var typePrefix = x.type.Contains(".")
                    ? x.type.Substring(0, x.type.IndexOf("."))
                    : x.type;

                return new KeyValuePair<string, string>($"{typePrefix} {x.variable.Replace("@@", "").ToLower()}", typemapper.ToCsDefinitionString(x));
            }).ToDictionary(x => x.Key, x => x.Value);
            string defMain = typemapper.ToCsDefinitionString(main);


            string desiredAssemblyLocation = Path.Combine(Build.ProjectPath, "bin", "debug");
            string desiredAssemblyName = Guid.NewGuid().ToString("N") + ".dll";
            var parser = new BironextIntegrationCsDefParser(desiredAssemblyLocation)
                                .BironextLazyIntegrationsCsDefParser(integrationName, desiredAssemblyName, integrationType);
            IMockWithInject<LazyIntegration> fromdef = parser.Create(defDeps, defMain);

            return fromdef;

        }

        public CurrentValue Adapt(IIntegration integ) {
            var typemapper = new Multimapper<IIntegration>(StaticVariables.desiredExtractedVariableTypes);
            CurrentValue curval = typemapper.ToCurrentValue(integ);
            return curval;
        }

        public Dictionary<string, object> InferredData(IIntegration integ)
        {
            var typemapper = new Multimapper<IIntegration>(StaticVariables.desiredExtractedVariableTypes);
            CurrentValue curval = typemapper.ToCurrentValue(integ);
            return typemapper.GetInferredState();
        }
    }




}