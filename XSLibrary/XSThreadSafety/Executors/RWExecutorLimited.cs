using System;
using System.Threading;

namespace XSLibrary.ThreadSafety.Executors
{
    public class RWExecutorLimited : SafeReadWriteExecutor
    {
        SafeReadExecutor m_readExecutor;
        SafeWriteExecutor m_writeExecutor;

        public int MaxParallelReaders { get { return m_writeExecutor.MaxParallelReaders; } }
        const int MAX_WRITERS = 1;

        public RWExecutorLimited(int maximumParallelReaders)
        {
            Semaphore writeLock = new Semaphore(MAX_WRITERS, MAX_WRITERS);
            Semaphore readLock = new Semaphore(maximumParallelReaders, maximumParallelReaders);

            m_writeExecutor = new SafeWriteExecutor(writeLock, readLock, maximumParallelReaders);
            m_readExecutor = new SafeReadExecutor(writeLock, readLock);
        }

        public override void Execute(Action executeFunction)
        {
            m_writeExecutor.Execute(executeFunction);
        }

        public override void ExecuteReadonly(Action executeFunction)
        {
            m_readExecutor.Execute(executeFunction);
        }

        public override ReturnType ExecuteReadonly<ReturnType>(Func<ReturnType> executeFunction)
        {
            return m_readExecutor.Execute(executeFunction);
        }

        public override void Lock()
        {
            m_writeExecutor.Lock();
        }

        public override void Release()
        {
            m_writeExecutor.Release();
        }

        public override void LockReadonly()
        {
            m_readExecutor.Lock();
        }

        public override void ReleaseReadonly()
        {
            m_readExecutor.Release();
        }
    }
}