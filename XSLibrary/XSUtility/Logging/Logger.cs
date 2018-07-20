using System;

namespace XSLibrary.Utility
{
    public abstract class Logger
    {
        public void Log(string text, params object[] arg)
        {
            Log(String.Format(text, arg));
        }
        public void Log(string text, object arg0)
        {
            Log(String.Format(text, arg0));
        }
        public abstract void Log(string text);
    }

    public class NoLog : Logger
    {
        public override void Log(string text)
        {
        }
    }

    public class LoggerConsole : Logger
    {
        public override void Log(string text)
        {
            Console.Out.WriteLine(text);
        }
    }

    public class LoggerConsolePeriodic : Logger
    {
        public string Prefix { get; set; } = "";
        public string Suffix { get; set; } = "";

        public override void Log(string text)
        {
            Console.Out.Write(Prefix + text + Suffix);
        }
    }
}
