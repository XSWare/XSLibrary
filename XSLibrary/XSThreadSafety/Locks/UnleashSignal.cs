using System;
using System.Threading;

namespace XSLibrary.ThreadSafety.Locks
{
    public class UnleashSignal : ILock, IDisposable
    {
        int currentlyWaiting = 0;
        private object m_lock = new object();
        volatile bool destroyed = false;

        public void Lock()
        {
            if (destroyed)
                return;

            lock (m_lock)
            {
                currentlyWaiting++;
                Monitor.Wait(m_lock);  // wait for a changed pulse
                currentlyWaiting--;
            }
        }

        public void Release()
        {
            lock (m_lock)
            {
                Monitor.PulseAll(m_lock);  // all waiting threads will resume once we release valueLock
            }
        }

        // destroy the lock, making every thread pass without waiting
        public void Destroy()
        {
            destroyed = true;

            while (true)
            {
                lock (m_lock)
                {
                    Monitor.PulseAll(m_lock);
                    if (currentlyWaiting == 0)
                        return;
                }
            }
        }

        public void Dispose()
        {
            Destroy();
        }
    }
}
