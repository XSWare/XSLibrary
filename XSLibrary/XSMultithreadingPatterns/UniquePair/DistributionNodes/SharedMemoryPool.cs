using System.Threading;

namespace XSLibrary.MultithreadingPatterns.UniquePair.DistributionNodes
{
    abstract public class SharedMemoryPool<PartType, GlobalDataType> : DistributionPool<PartType, GlobalDataType>
    {
        protected ManualResetEvent[] ResetEvents { get; set; }
        protected SharedMemoryStackCalculation<PartType, GlobalDataType> CalculationLogic { get; private set; }

        public SharedMemoryPool(int nodeCount)
        {
            CalculationLogic = new SharedMemoryStackCalculation<PartType, GlobalDataType>();

            ResetEvents = new ManualResetEvent[nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                ResetEvents[i] = new ManualResetEvent(false);
            }
        }

        public void SetCalculationFunction(SharedMemoryStackCalculation<PartType, GlobalDataType>.PairCalculationFunction calculationFunction)
        {
            CalculationLogic.SetCalculationFunction(calculationFunction);
        }
    }
}
