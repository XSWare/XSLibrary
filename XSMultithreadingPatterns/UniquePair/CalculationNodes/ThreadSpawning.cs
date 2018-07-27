using System.Threading;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public class ThreadSpawning<PartType, GlobalDataType> : CorePool<PartType, GlobalDataType>
    {
        public ThreadSpawning(int threadCount) : base(threadCount)
        {
        }

        protected override void ExecuteOnCore(int coreIndex, PairingData<PartType, GlobalDataType> calculationPair)
        {
            new Thread(
                () =>
                {
                    CalculationLogic.Calculate(calculationPair);
                    ResetEvents[coreIndex].Set();
                }
                ).Start();
        }

        public override void Dispose()
        {
        }
    }
}
