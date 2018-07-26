using System.Threading;

namespace XSLibrary.ThreadSafety.Locks
{
    public class SingleLock : ILock
    {
        bool shared;
        Semaphore m_lock;

        public SingleLock() : this(new Semaphore(1, 1))
        {
            shared = false;
        }
        public SingleLock(Semaphore sharedLock)
        {
            shared = true;
            m_lock = sharedLock;
        }

        public void Lock()
        {
            m_lock.WaitOne();
        }

        public void Release()
        {
            m_lock.Release();
        }

        public void Dispose()
        {
            if (shared)
                m_lock.Dispose();
        }
    }
}
