using System.Threading;

namespace XSLibrary.ThreadSafety.Locks
{
    public class UnleashSignal : ILock
    {
        private object m_lock = new object();

        public void Lock()
        {
            lock (m_lock)
            {
                Monitor.Wait(m_lock);  // wait for a changed pulse
            }
        }

        public void Release()
        {
            lock (m_lock)
            {
                Monitor.PulseAll(m_lock);  // all waiting threads will resume once we release valueLock
            }
        }
    }
}
