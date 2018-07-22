using System;
using System.Threading;

namespace XSLibrary.Utility
{
    public class ThreadStarter
    {
        public static void ThreadpoolDebug(string threadName, Action threadStart)
        {
#if DEBUG
            Thread connectThread = new Thread(() => threadStart());
            connectThread.Name = threadName;
            connectThread.Start();
#else
            ThreadPool.QueueUserWorkItem((state) => threadStart());
#endif
        }
    }
}
