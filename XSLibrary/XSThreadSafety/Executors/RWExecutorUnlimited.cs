using System;
using System.Threading;

namespace XSLibrary.ThreadSafety.Executors
{
    public class RWExecutorUnlimited : SafeReadWriteExecutor
    {
        volatile int m_readerCount, m_writerCount;
        Semaphore m_readEntry, m_writeEntry, m_readLock, m_resourceLock;

        public RWExecutorUnlimited()
        {
            m_readerCount = 0;
            m_writerCount = 0;

            m_readEntry = new Semaphore(1, 1);
            m_writeEntry = new Semaphore(1, 1);
            m_readLock = new Semaphore(1, 1);
            m_resourceLock = new Semaphore(1, 1);
        }

        public override void Lock()
        {
            m_writeEntry.WaitOne(); // avoid race conditions with other writers in the entry section

            m_writerCount++;
            if (m_writerCount == 1)
                m_readLock.WaitOne(); // first writer must lock out new readers - readers who are already in the access process will finish

            m_writeEntry.Release();

            m_resourceLock.WaitOne(); // lock resource so only one writer can access it simultaneously

            //Console.Out.WriteLine("[EXECUTOR] Write locked.");
        }

        public override void Release()
        {
            //Console.Out.WriteLine("[EXECUTOR] Write released.");

            m_resourceLock.Release(); // other writers can continue, readers are still locked out

            m_writeEntry.WaitOne(); // avoid race conditions with other writers in the exit section

            m_writerCount--;
            if (m_writerCount == 0)
                m_readLock.Release(); // last writer must allow the readers to access the resource again

            m_writeEntry.Release();
        }

        public override void LockReadonly()
        {
            m_readLock.WaitOne(); // can only pass if there is no writer currently accessing the resource
            // at this point the reader can not be blocked from accessing the resource and it will continue until the resource was read
            m_readEntry.WaitOne(); // avoid race conditions with other readers in the entry section

            m_readerCount++;
            if (m_readerCount == 1)
                m_resourceLock.WaitOne(); // first reader locks resource so no writer can access it, all other readers will not call this and just continue

            m_readEntry.Release();

            m_readLock.Release();

            //Console.Out.WriteLine("[EXECUTOR] Read locked.");
        }

        public override void ReleaseReadonly()
        {
            //Console.Out.WriteLine("[EXECUTOR] Read released - {0} are still reading.", m_readerCount - 1);

            m_readEntry.WaitOne(); // avoid race conditions with other readers in the exit section

            m_readerCount--;
            if (m_readerCount == 0)
                m_resourceLock.Release(); // last reader must allow the writers to access the resource again

            m_readEntry.Release();//release exit section for other readers
        }
    }
}
