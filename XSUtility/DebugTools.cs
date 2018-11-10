using System;
using System.Threading;

namespace XSLibrary.Utility
{
    public class DebugTools
    {
        // use the thread pool in release mode but use named threads in debug
        public static void ThreadpoolStarter(string threadName, Action threadStart)
        {
#if DEBUG
            Thread connectThread = new Thread(() => threadStart());
            connectThread.Name = threadName;
            connectThread.Start();
#else
            ThreadPool.QueueUserWorkItem((state) => threadStart());
#endif
        }

        // catches all exceptions in release mode but bubbles them in debug
        public static void CatchAll(Action throwingAction)
        {
#if !DEBUG
            try
            {
#endif
                throwingAction();
#if !DEBUG
            }
            catch { }
#endif
        }
    }
}
