using System.Collections.Generic;
using tests.composition.root_builder;
using si.birokrat.next.common.logging;

namespace tests.composition.fixed_integration.fixed_task.fixed_workload_object_source.deps
{
    public class LoggerService
    {
        private IMyLoggerFactory loggerFactory;
        private Dictionary<string, IMyLogger> loggers = new Dictionary<string, IMyLogger>();

        public LoggerService(IMyLoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public Dictionary<string, IMyLogger> CreateLoggers(List<string> names)
        {
            names.ForEach(x => loggers[x] = loggerFactory.Create());
            return loggers;
        }
    }
}
