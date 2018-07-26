using System.Threading;

namespace XSLibrary.ThreadSafety.Locks
{
    public class UnleashSignal : ILock
    {
        private volatile int currentWait = 0;
        private object m_lock = new object();

        public void Lock()
        {
            lock (m_lock)
            {
                int waitOn = currentWait + 1;

                while (true)
                {
                    if (waitOn == currentWait)
                        return;  // no race condition here: PulseAll can only be reached once we hit Wait()

                    Monitor.Wait(m_lock);  // wait for a changed pulse
                }
            }
        }

        public void Release()
        {
            lock (m_lock)
            {
                currentWait++;
                Monitor.PulseAll(m_lock);  // all waiting threads will resume once we release valueLock
            }
        }
    }
}
