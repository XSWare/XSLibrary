using System.Threading;

namespace XSLibrary.ThreadSafety.Executors
{
    public class SingleThreadExecutor : SafeExecutor
    {
        Semaphore m_lock;

        public SingleThreadExecutor() : this(new Semaphore(1, 1)) { }
        public SingleThreadExecutor(Semaphore sharedLock)
        {
            m_lock = sharedLock;
        }

        public override void Lock()
        {
            m_lock.WaitOne();
        }

        public override void Release()
        {
            m_lock.Release();
        }
    }
}