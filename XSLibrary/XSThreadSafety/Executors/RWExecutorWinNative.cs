using System;
using System.Threading;

namespace XSLibrary.ThreadSafety.Executors
{
    public class RWExecutorWinNative : SafeReadWriteExecutor
    {
        ReaderWriterLock m_readWriteLock;

        public RWExecutorWinNative()
        {
            m_readWriteLock = new ReaderWriterLock();
        }

        const int INFINITE = -1;

        public override void Lock()
        {
            m_readWriteLock.AcquireWriterLock(INFINITE);
            Console.Out.WriteLine("[EXECUTOR] Write locked.");
        }

        public override void Release()
        {
            Console.Out.WriteLine("[EXECUTOR] Write released.");
            m_readWriteLock.ReleaseWriterLock();
        }

        public override void LockReadonly()
        {
            m_readWriteLock.AcquireReaderLock(INFINITE);
            Console.Out.WriteLine("[EXECUTOR] Read locked.");
        }

        public override void ReleaseReadonly()
        {
            Console.Out.WriteLine("[EXECUTOR] Read released.");
            m_readWriteLock.ReleaseReaderLock();
        }
    }
}
