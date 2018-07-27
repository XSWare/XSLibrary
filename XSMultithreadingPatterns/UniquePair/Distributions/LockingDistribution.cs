using System.Threading;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public abstract class LockingDistribution<PartType, GlobalDataType> : StandaloneDistribution<PartType, GlobalDataType>
    {
        protected PartType[] m_elements;
        protected GlobalDataType m_global;
        Semaphore[] m_locks;
        ManualResetEvent[] m_waitHandles;

        public LockingDistribution(int coreCount) : base(coreCount)
        {
            m_waitHandles = new ManualResetEvent[CoreCount];
            for (int i = 0; i < CoreCount; i++)
                m_waitHandles[i] = new ManualResetEvent(false);
        }

        public override void Calculate(PartType[] elements, GlobalDataType globalData)
        {
            ResetWaitHandles();

            if (m_elements == null || m_elements.Length != elements.Length)
                ResetLocks(elements.Length);

            m_elements = elements;
            m_global = globalData;

            for (int i = 0; i < CoreCount; i++)
            {
                ThreadPool.QueueUserWorkItem(ThreadExecution, i);
            }

            WaitHandle.WaitAll(m_waitHandles);
        }

        private void ResetWaitHandles()
        {
            for (int i = 0; i < CoreCount; i++)
                m_waitHandles[i].Reset();
        }

        private void ResetLocks(int elementCount)
        {
            m_locks = new Semaphore[elementCount];
            for (int i = 0; i < elementCount; i++)
                m_locks[i] = new Semaphore(1, 1);
        }

        private void ThreadExecution(object wrappedData)
        {
            int threadID = (int)wrappedData;

            Distribute(threadID);

            m_waitHandles[threadID].Set();
        }

        protected abstract void Distribute(int coreID);

        protected void CalculatePair(int id1, int id2)
        {
            m_locks[id1].WaitOne();
            m_locks[id2].WaitOne();

            CalculationFunction(m_elements[id1], m_elements[id2], m_global);

            m_locks[id2].Release();
            m_locks[id1].Release();
        }

        public override void Dispose()
        {
        }
    }
}