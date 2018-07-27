using System;

namespace XSLibrary.Utility
{
    public enum LogLevel
    {
        Detail,
        Information,
        Warning,
        Error,
        Priority
    }

    public abstract class Logger : IDisposable
    {
        public static LogLevel DefaultLogLevel { get; set; } = LogLevel.Warning;
        public LogLevel LogLevel { get; set; } = DefaultLogLevel;
        public static Logger NoLog { get; private set; } = new NoLog();

        public void Log(LogLevel logLevel, string text, params object[] arg)
        {
            Log(logLevel, String.Format(text, arg));
        }
        public void Log(LogLevel logLevel, string text, object arg0)
        {
            Log(logLevel, String.Format(text, arg0));
        }

        public virtual void Log(LogLevel logLevel, string text)
        {
            if (logLevel >= LogLevel)
                LogMessage(text);
        }

        protected virtual void LogMessage(string text) { }

        public virtual void Dispose()
        {

        }
    }

    class NoLog : Logger
    {
        protected override void LogMessage(string text)
        {
        }
    }

    public class LoggerConsole : Logger
    {
        protected override void LogMessage(string text)
        {
            Console.Out.WriteLine(text);
        }
    }

    public class LoggerConsolePeriodic : Logger
    {
        public string Prefix { get; set; }
        public string Suffix { get; set; }

        public LoggerConsolePeriodic() : this("", "") { }
        public LoggerConsolePeriodic(string prefix, string suffix)
        {
            Prefix = prefix;
            Suffix = suffix;
        }

        protected override void LogMessage(string text)
        {
            Console.Out.Write(Prefix + text + Suffix);
        }
    }
}
