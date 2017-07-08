using System;
using System.Threading;

namespace XSLibrary.ThreadSafety.Executors
{
    class SafeReadExecutor : SafeExecutor
    {
        Semaphore m_writeLock;
        Semaphore m_readLock;

        public SafeReadExecutor(Semaphore writeLock, Semaphore readLock)
        {
            m_writeLock = writeLock;
            m_readLock = readLock;
        }

        public override void Lock()
        {
            m_writeLock.WaitOne();
            m_readLock.WaitOne();
            m_writeLock.Release();
#if DEBUG
            Console.Out.WriteLine("[EXECUTOR] Read locked.");
#endif
        }

        public override void Release()
        {
#if DEBUG
            Console.Out.WriteLine("[EXECUTOR] Read released.");
#endif
            m_readLock.Release();
        }
    }
}