using System.Collections.Generic;

namespace XSLibrary.Utility
{
    class MultiLogger : Logger
    {
        public List<Logger> Logs { get; private set; }

        public MultiLogger() : this(new List<Logger>()) { }
        public MultiLogger(List<Logger> logs)
        {
            Logs = logs;
        }

        public override void Log(LogLevel logLevel, string text)
        {
            foreach (Logger logger in Logs)
                logger.Log(logLevel, text);
        }
    }
}
