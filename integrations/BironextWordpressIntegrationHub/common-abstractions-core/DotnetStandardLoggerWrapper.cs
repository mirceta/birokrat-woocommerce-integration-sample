using common_abstractions_std.wrappers;
using Microsoft.Extensions.Logging;
using System;

namespace common_abstractions_core
{
    public class DotnetStandardLoggerWrapper : common_abstractions_std.wrappers.ILogger
    {

        Microsoft.Extensions.Logging.ILogger logger;

        public DotnetStandardLoggerWrapper(Microsoft.Extensions.Logging.ILogger logger) {
            this.logger = logger;
        }
        public void LogWarning(string message) {
            if (logger != null)
                logger.LogWarning(message);
        }
    }
}
