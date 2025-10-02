using BiroWooHub.logic.integration;
using core.customers;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Collections.Generic;
using si.birokrat.next.common.build;
using System.IO;

namespace gui_generator.cs_definition_serializer
{
    public class BironextIntegrationCsDefParser
    {


        string desiredAssemblyLocation;
        public BironextIntegrationCsDefParser(string desiredAssemblyLocation)
        {
            this.desiredAssemblyLocation = desiredAssemblyLocation;
        }

        public CsDefinitionParser<IIntegration> BironextIntegrationsCsDefParser()
        {
            var assemblyReferences = AssemblyReferences();
            var compiler = new RuntimeCompilation(assemblyReferences, Path.Combine(Build.ProjectPath, "some.dll"));
            var csdefparser = new CsDefinitionParser<IIntegration>(compiler, usingStatements(), "core");
            return csdefparser;
        }

        public CsDefinitionParser<LazyIntegration> BironextLazyIntegrationsCsDefParser(string integrationName,
            string assemblyName, string integrationType)
        {
            var assemblyReferences = AssemblyReferences();
            var compiler = new RuntimeCompilation(assemblyReferences, Path.Combine(desiredAssemblyLocation, assemblyName));
            var csdefparser = new CsDefinitionParser<LazyIntegration>(compiler, usingStatements(), "core",
                                    new LazyIntegrationTemplateGenerator(usingStatements(), "core", integrationName, integrationType));
            var x = assemblyReferences.ToList();
            return csdefparser;
        }

        private static IEnumerable<MetadataReference> AssemblyReferences()
        {
            var firstLevelReferences = typeof(PredefinedIntegrationFactory).Assembly.GetReferencedAssemblies();
            var allReferences = new HashSet<AssemblyName>(firstLevelReferences);

            // Direct references (depth 1)
            foreach (var assemblyName in firstLevelReferences)
            {
                LoadReferencedAssemblies(assemblyName, allReferences, 1, 1); // Start depth from 1, max depth is 2
            }

            var references = allReferences
                .Select(name => MetadataReference.CreateFromFile(Assembly.Load(name).Location))
                .ToList();

            // Adding hardcoded reference
            var hardcodedReference = MetadataReference.CreateFromFile(Path.Combine(Build.SolutionPath, "gui_inferable\\bin\\Debug\\netstandard2.0\\gui_inferable.dll"));
            references.Add(hardcodedReference);

            return references;
        }

        private static void LoadReferencedAssemblies(AssemblyName assemblyName, HashSet<AssemblyName> allReferences, int currentDepth, int maxDepth)
        {
            if (currentDepth > maxDepth)
            {
                return;
            }

            Assembly assembly;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (FileNotFoundException)
            {
                // Handle or log the exception as necessary
                return; // Skip loading this assembly if it's not found
            }

            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                if (allReferences.Add(referencedAssembly)) // Add returns true if the item was added
                {
                    // Recursively load referenced assemblies with increased depth
                    LoadReferencedAssemblies(referencedAssembly, allReferences, currentDepth + 1, maxDepth);
                }
            }
        }


        private static string usingStatements()
        {
            return @"
            using System;
            using System.Collections.Generic;
            using BirokratNext;
            using BiroWoocommerceHub;
            using BiroWoocommerceHub.structs_wc_to_biro;
            using BiroWoocommerceHubTests;
            using BiroWooHub.logic.integration;
            using core.customers.spicasport;
            using core.logic.common_birokrat;
            using core.logic.common_woo;
            using core.logic.mapping_biro_to_woo;
            using core.logic.mapping_biro_to_woo.change_handlers;
            using core.structs;
            using core.tools.zalogaretriever;
            using tests.tools;
            using webshop_client_woocommerce;
            using BiroWoocommerceHubTests.tools;
            using core;
            using core.tools.attributemapper;
            using core.zgeneric;
            using core.customers;
            using core.customers.zgeneric;
            using core.logic.mapping_woo_to_biro.document_insertion;
            using BiroWoocommerceHub.logic;
            using core.customers.zgeneric.order_operations;
            using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
            using allintegrations.customers;
            using core.logic.mapping_woo_to_biro.order_operations;
            using core.logic.mapping_woo_to_biro.document_insertion.postavke_extractors;
            using core.logic.mapping_woo_to_biro;
            using gui_inferable;
            using core.customers.poledancerka.mappers;
            using core.logic.mapping_woo_to_biro.product_ops;
            using ApiClient.utils;
            using gui_generator.api;
            using core.logic.mapping_woo_to_biro.order_operations.pl;
            using core.logic.mapping_woo_to_biro.orderflow.order_operations;

            using core.customers.poledancerka;
            using BironextWordpressIntegrationHub;
            using JsonIntegrationLoader.utils;
            ";
        }
    }


}
