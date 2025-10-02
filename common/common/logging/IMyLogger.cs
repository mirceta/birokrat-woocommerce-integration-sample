using System;
using System.Collections.Generic;
using System.Text;

namespace si.birokrat.next.common.logging {

    public interface IMyLoggable {

        void SetLogger(IMyLogger logger);
    }

    public class MyLoggableHelper {

        public static void SetLogger(IMyLoggable loggable, IMyLogger logger) {
            if (loggable == null)
                throw new ArgumentNullException("loggable");
            if (logger == null)
                throw new ArgumentNullException("logger");
            loggable.SetLogger(logger);
        }
    }

    public abstract class MyLoggable : IMyLoggable {

        public IMyLogger logger;

        public MyLoggable() {
            logger = new ConsoleMyLogger();
        }
        
        public void SetLogger(IMyLogger logger) {
            if (logger == null)
                throw new ArgumentNullException("logger");
            this.logger = logger;
        }
    }

    public interface IMyLogger {
        void LogInformation(string message);
        void LogWarning(string message, Exception ex = null);
        void LogError(string message, Exception ex = null);
    }

    public class ConsoleMyLogger : IMyLogger {
        public void LogError(string message, Exception ex = null) {
            string msg = "";
            if (ex != null)
                msg = ex.Message;
            Console.WriteLine(message + msg);
        }

        public void LogInformation(string message) {
            Console.WriteLine(message);
        }

        public void LogWarning(string message, Exception ex = null) {
            string msg = "";
            if (ex != null)
                msg = ex.Message;
            Console.WriteLine(message + msg);
        }
    }

    public class ConsoleLoggerDecorator : IMyLogger
    {
        IMyLogger next;
        ConsoleMyLogger logger;
        public ConsoleLoggerDecorator(IMyLogger next) {
            if (next == null)
                throw new ArgumentNullException("next");
            this.next = next;
            logger = new ConsoleMyLogger();
        }
        
        public void LogError(string message, Exception ex = null)
        {
            logger.LogError(message, ex);
            next.LogError(message, ex);
        }

        public void LogInformation(string message)
        {
            logger.LogInformation(message);
            next.LogInformation(message);
        }

        public void LogWarning(string message, Exception ex = null)
        {
            logger.LogWarning(message, ex);
            next.LogError(message, ex);
        }
    }
}
