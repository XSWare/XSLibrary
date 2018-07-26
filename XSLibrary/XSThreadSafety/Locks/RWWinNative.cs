using System.Threading;

namespace XSLibrary.ThreadSafety.Locks
{
    public class RWWinNative : IReadWriteLock
    {
        ReaderWriterLock m_lock;
        LockCookie lockCookie;

        const int INFINITE = -1;

        public RWWinNative() : this(new ReaderWriterLock()) { }
        public RWWinNative(ReaderWriterLock rwLock)
        {
            m_lock = rwLock;
        }

        public void Lock()
        {
            m_lock.AcquireWriterLock(INFINITE);
        }

        public void Release()
        {
            m_lock.ReleaseWriterLock();
        }

        public void LockRead()
        {
            m_lock.AcquireReaderLock(INFINITE);
        }

        public void ReleaseRead()
        {
            m_lock.ReleaseReaderLock();
        }

        public void UpgradeToWrite()
        {
            lockCookie = m_lock.UpgradeToWriterLock(INFINITE);
        }

        public void DowngradeToRead()
        {
            m_lock.DowngradeFromWriterLock(ref lockCookie);
        }

        public void Dispose()
        {

        }
    }
}
