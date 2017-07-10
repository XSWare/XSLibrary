using System.Threading;

namespace XSLibrary.MultithreadingPatterns.UniquePair.DistributionNodes
{
    public class SystemHandledThreadPool<PartType, GlobalDataType> : SharedMemoryPool<PartType, GlobalDataType>
    {
        class ThreadCalculationData
        {
            public CalculationPair<PartType, GlobalDataType> m_pair;
            public ManualResetEvent m_resetEvent;
        }

        public override int NodeCount { get { return ThreadCount; } }
        int ThreadCount { get; set; }

        public SystemHandledThreadPool(int threadCount) : base(threadCount)
        {
            ThreadCount = threadCount;
        }

        public override void DistributeCalculation(int nodeIndex, CalculationPair<PartType, GlobalDataType> calculationPair)
        {
            ResetEvents[nodeIndex].Reset();
            ThreadCalculationData data = new ThreadCalculationData()
            {
                m_pair = calculationPair,
                m_resetEvent = ResetEvents[nodeIndex]
            };
            ThreadPool.QueueUserWorkItem(CalculationCallback, data);
        }

        private void CalculationCallback(object state)
        {
            ThreadCalculationData data = state as ThreadCalculationData;
            CalculationLogic.Calculate(data.m_pair);
            data.m_resetEvent.Set();
        }

        public override void Synchronize()
        {
            WaitHandle.WaitAll(ResetEvents);
        }

        public override void Dispose()
        {
        }
    }
}
