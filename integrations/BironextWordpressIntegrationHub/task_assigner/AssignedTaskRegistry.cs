using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using task_assigner;

namespace BironextWordpressIntegrationHub.Controllers
{
    // dependency injection inject, and define all shit that gets registered in here!

    public interface IAssignedTaskRegistry {
        Dictionary<string, IAssignedTaskExtension> GetExtensions();
        void Register(string type, IAssignedTaskFrontendFactory factory);
        IAssignedTaskFrontendFactory Get(string type);
    }

    public class Cached : IAssignedTaskRegistry
    {

        Dictionary<string, IAssignedTaskExtension> extensions = null;
        IAssignedTaskRegistry next;
        public Cached(IAssignedTaskRegistry next) {
            this.next = next;
        }

        public IAssignedTaskFrontendFactory Get(string type)
        {
            return next.Get(type);
        }

        public Dictionary<string, IAssignedTaskExtension> GetExtensions()
        {
            if (extensions == null)
                extensions = next.GetExtensions();
            return extensions;
        }

        public void Register(string type, IAssignedTaskFrontendFactory factory)
        {
            next.Register(type, factory);
        }
    }

    public class AssignedTaskRegistry : IAssignedTaskRegistry{

        Dictionary<string, IAssignedTaskFrontendFactory> registry;
        
        public AssignedTaskRegistry() { 
            registry = new Dictionary<string, IAssignedTaskFrontendFactory>();
        }

        public void Register(string type, IAssignedTaskFrontendFactory factory) {
            registry[type] = factory;
        }

        public IAssignedTaskFrontendFactory Get(string type)
        {
            return registry[type];
        }

        public Dictionary<string, IAssignedTaskExtension> GetExtensions() {

            var dictionaries = registry.Select(x =>
                x.Value.GetExtensions().ToDictionary(x => x.Path, x => x));

            var result = dictionaries
              .SelectMany(dict => dict) // Flatten key-value pairs from all dictionaries
              .ToDictionary(pair => pair.Key, pair => pair.Value); // Create a new dictionary
            return result;
        }
    }
}


