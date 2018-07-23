using System.Threading;

namespace XSLibrary.ThreadSafety.Locks
{
    public class SingleLock : ILock
    {
        Semaphore m_lock;

        public SingleLock() : this(new Semaphore(1, 1)) { }
        public SingleLock(Semaphore sharedLock)
        {
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
    }
}
