using XSLibrary.ThreadSafety.Containers;

namespace XSLibrary.Utility
{
    public class MultiLogger : Logger
    {
        public SafeList<Logger> Logs { get; private set; }

        public MultiLogger() : this(new SafeList<Logger>()) { }
        public MultiLogger(SafeList<Logger> logs)
        {
            Logs = logs;
        }

        public override void Log(LogLevel logLevel, string text)
        {
            foreach (Logger logger in Logs.Entries)
                logger.Log(logLevel, text);
        }

        public override void Dispose()
        {
            foreach (Logger logger in Logs.Entries)
            {
                if(Logs.Remove(logger))
                    logger.Dispose();
            }
        }
    }
}
