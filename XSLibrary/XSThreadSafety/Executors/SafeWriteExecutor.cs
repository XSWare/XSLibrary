using System;
using System.Threading;

namespace XSLibrary.ThreadSafety.Executors
{
    class SafeWriteExecutor : SafeExecutor
    {
        Semaphore m_writeLock;
        Semaphore m_readLock;

        public int MaxParallelReaders { get; private set; }

        public SafeWriteExecutor(Semaphore writeLock, Semaphore readLock, int maxParallelReads)
        {
            MaxParallelReaders = maxParallelReads;

            m_writeLock = writeLock;
            m_readLock = readLock;
        }

        public override void Lock()
        {
            m_writeLock.WaitOne();
#if DEBUG
            Console.Out.WriteLine("[EXECUTOR] Write locked.");
#endif
            for (int i = 0; i < MaxParallelReaders; i++)
                m_readLock.WaitOne();
#if DEBUG
            Console.Out.WriteLine("[EXECUTOR] All reads locked.");
#endif
        }

        public override void Release()
        {
#if DEBUG
            Console.Out.WriteLine("[EXECUTOR] Write released.");
#endif
            m_writeLock.Release();
            for (int i = 0; i < MaxParallelReaders; i++)
                m_readLock.Release();
        }
    }
}