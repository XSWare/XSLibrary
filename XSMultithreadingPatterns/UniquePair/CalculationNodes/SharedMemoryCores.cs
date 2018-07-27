using System.Threading;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    abstract public class CorePool<PartType, GlobalDataType> : CorePoolBase<PartType, GlobalDataType>
    {
        protected ManualResetEvent[] ResetEvents { get; set; }
        protected SharedMemoryStackCalculation<PartType, GlobalDataType> CalculationLogic { get; private set; }

        int m_coreCount;
        public override int CoreCount { get { return m_coreCount; } }

        public CorePool(int coreCount)
        {
            m_coreCount = coreCount;

            CalculationLogic = new SharedMemoryStackCalculation<PartType, GlobalDataType>();

            ResetEvents = new ManualResetEvent[coreCount];
            for (int i = 0; i < coreCount; i++)
            {
                ResetEvents[i] = new ManualResetEvent(true);
            }
        }

        sealed public override void DistributeCalculation(int coreIndex, PairingData<PartType, GlobalDataType> calculationPair)
        {
            ResetEvents[coreIndex].Reset();
            ExecuteOnCore(coreIndex, calculationPair);
        }

        protected abstract void ExecuteOnCore(int coreIndex, PairingData<PartType, GlobalDataType> calculationPair);

        public void SetCalculationFunction(UniquePairDistribution<PartType, GlobalDataType>.PairCalculationFunction calculationFunction)
        {
            CalculationLogic.SetCalculationFunction(calculationFunction);
        }

        public override void Synchronize()
        {
            WaitHandle.WaitAll(ResetEvents);
        }

        public override void Synchronize(int nodeIndex)
        {
            ResetEvents[nodeIndex].WaitOne();
        }
    }
}
