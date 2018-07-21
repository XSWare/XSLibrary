using System;

namespace XSLibrary.Utility.Logging
{
    public class LambdaLogger : Logger
    {
        Action<string> m_log;

        public LambdaLogger(Action<string> log)
        {
            m_log = log;
        }

        protected override void LogMessage(string text)
        {
            m_log(text);
        }
    }
}
