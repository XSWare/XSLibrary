using System.Threading;

namespace XSLibrary.ThreadSafety.Locks
{
    public class RWLock : IReadWriteLock
    {
        volatile int m_readerCount, m_writerCount;
        Semaphore m_readEntry, m_writeEntry, m_readLock, m_resourceLock;

        public RWLock()
        {
            m_readerCount = 0;
            m_writerCount = 0;

            m_readEntry = new Semaphore(1, 1);
            m_writeEntry = new Semaphore(1, 1);
            m_readLock = new Semaphore(1, 1);
            m_resourceLock = new Semaphore(1, 1);
        }

        public void Lock()
        {
            m_writeEntry.WaitOne(); // avoid race conditions with other writers in the entry section

            m_writerCount++;
            if (m_writerCount == 1)
                m_readLock.WaitOne(); // first writer must lock out new readers - readers who are already in the access process will finish

            m_writeEntry.Release();

            m_resourceLock.WaitOne(); // lock resource so only one writer can access it simultaneously
        }

        public void Release()
        {
            m_resourceLock.Release(); // other writers can continue, readers are still locked out

            m_writeEntry.WaitOne(); // avoid race conditions with other writers in the exit section

            m_writerCount--;
            if (m_writerCount == 0)
                m_readLock.Release(); // last writer must allow the readers to access the resource again

            m_writeEntry.Release();
        }

        public void LockRead()
        {
            m_readLock.WaitOne(); // can only pass if there is no writer currently accessing the resource
            // at this point the reader can not be blocked from accessing the resource and it will continue until the resource was read
            m_readEntry.WaitOne(); // avoid race conditions with other readers in the entry section

            m_readerCount++;
            if (m_readerCount == 1)
                m_resourceLock.WaitOne(); // first reader locks resource so no writer can access it, all other readers will not call this and just continue

            m_readEntry.Release();

            m_readLock.Release();
        }

        public void ReleaseRead()
        {
            m_readEntry.WaitOne(); // avoid race conditions with other readers in the exit section

            m_readerCount--;
            if (m_readerCount == 0)
                m_resourceLock.Release(); // last reader must allow the writers to access the resource again

            m_readEntry.Release();//release exit section for other readers
        }

        public void UpgradeToWrite()
        {
            m_writeEntry.WaitOne(); // lock out all writers from locking or releasing

            m_writerCount++;    // is now a writer and must increment count
            if (m_writerCount == 1)
                m_readLock.WaitOne(); // first writer must lock out new readers - readers who are already in the access process will finish

            m_readEntry.WaitOne();  // lock to not have race condtions with other releasing readers

            m_readerCount--;        // not a reader anymore
            if (m_readerCount == 0)     // must wait if it's not the last reader, else we just keep the resource lock we already have
            {
                m_readEntry.Release();  // have the ressource lock and are the last reader -> can continue in write mode
            }
            else
            {
                m_readEntry.Release();      // release so other readers can finish
                m_resourceLock.WaitOne();   // wait until all readers are finished
            }

            m_writeEntry.Release();
        }

        public void DowngradeToRead()
        {
            m_readEntry.WaitOne();
            m_readerCount++;        // currently a writer so the ressource lock is always acquired - no need to get it
            m_readEntry.Release();

            m_writeEntry.WaitOne(); // avoid race conditions with other writers in the exit section
            m_writerCount--;
            if (m_writerCount == 0)
                m_readLock.Release(); // last writer must allow the readers to access the resource again

            m_writeEntry.Release();
        }
    }
}
