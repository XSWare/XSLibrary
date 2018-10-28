using System;
using System.Threading;

namespace XSLibrary.ThreadSafety.Locks
{
    public class UnleashSignal : ILock, IDisposable
    {
        // The UnleashSignal is basically like the AutoResetEvent with the additional convenience of having multiple Threads being able to wait for the same release.
        // With the AutoResetEvent if multiple threads are using the same AutoResetEvent and are calling the WaitOne-Function only one thread will be able to continue.
        // With the UnleashSignal all of the threads currently in the Lock method will be able to continue once released is called.
        int currentlyWaiting = 0;
        private object m_lock = new object();
        volatile bool destroyed = false;

        public void Lock()
        {
            lock (m_lock)
            {
                if (destroyed)
                    return;

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
