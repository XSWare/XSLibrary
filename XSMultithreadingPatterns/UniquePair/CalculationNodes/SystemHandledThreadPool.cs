using System.Threading;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public class SystemHandledThreadPool<PartType, GlobalDataType> : CorePool<PartType, GlobalDataType>
    {
        class ThreadCalculationData
        {
            public PairingData<PartType, GlobalDataType> m_pair;
            public ManualResetEvent m_resetEvent;
        }

        public SystemHandledThreadPool(int threadCount) : base(threadCount)
        {
        }

        protected override void ExecuteOnCore(int coreIndex, PairingData<PartType, GlobalDataType> calculationPair)
        {
            ThreadCalculationData data = new ThreadCalculationData()
            {
                m_pair = calculationPair,
                m_resetEvent = ResetEvents[coreIndex]
            };
            ThreadPool.QueueUserWorkItem(CalculationCallback, data);
        }

        private void CalculationCallback(object state)
        {
            ThreadCalculationData data = state as ThreadCalculationData;
            CalculationLogic.Calculate(data.m_pair);
            data.m_resetEvent.Set();
        }

        public override void Dispose()
        {
        }
    }
}
