using BiroWooHub.logic.integration;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.customers {
    public class LazyIntegration {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }

        public Dictionary<string, string> AdditionalInfo { get; set; }

        public IMyLogger Logger { get; set; }
        
        Func<Task<IIntegration>> integ;
        IIntegration evaled = null;
        public Func<Task<IIntegration>> BuildIntegrationAsync { get {
                return async () => {
                    if (evaled == null) {
                        var integration = await integ();

                        if (Logger != null) {
                            integration.BiroClient.SetLogger(Logger);
                            integration.WooClient.SetLogger(Logger);
                        }

                        integration.ExternalInfo = AdditionalInfo;
                        evaled = integration;
                        return integration;
                    } else {
                        return evaled;
                    }
                };
            } set => integ = value; }
        
    }
}