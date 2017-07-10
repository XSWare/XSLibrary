using System.Threading;

namespace XSLibrary.MultithreadingPatterns.UniquePair.DistributionNodes
{
    public partial class ActorPool<PartType, GlobalDataType> : DistributionPool<PartType, GlobalDataType>
    {
        public override int NodeCount { get { return PoolSize; } }
        private int PoolSize { get; set; }
        public bool FixedCores { get; private set; }
        ActorNode[] Actors { get; set; }
        ManualResetEvent[] ResetEvents { get; set; }
        SharedMemoryStackCalculation<PartType, GlobalDataType> CalculationLogic { get; set; }

        public ActorPool(int size, bool fixedCores)
        {
            CalculationLogic = new SharedMemoryStackCalculation<PartType, GlobalDataType>();
            PoolSize = size;
            FixedCores = fixedCores;
            InitializeActors();
        }

        public void SetCalculationFunction(SharedMemoryStackCalculation<PartType, GlobalDataType>.PairCalculationFunction calculationFunction)
        {
            CalculationLogic.SetCalculationFunction(calculationFunction);
        }

        public override void DistributeCalculation(int nodeIndex, CalculationPair<PartType, GlobalDataType> calculationPair)
        {
            ResetEvents[nodeIndex].Reset();
            Actors[nodeIndex].CalculateStacks(calculationPair);
        }

        public override void Synchronize()
        {
            WaitHandle.WaitAll(ResetEvents);
        }

        public override void Dispose()
        {
            Close(true);
        }

        public void Close(bool join = false)
        {
            foreach (ActorNode actor in Actors)
            {
                actor.Stop();
            }

            if (join)
                Synchronize();
        }

        private void InitializeActors()
        {
            ResetEvents = new ManualResetEvent[PoolSize];
            Actors = new ActorNode[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                ResetEvents[i] = new ManualResetEvent(false);
                Actors[i] = new ActorNode(CalculationLogic, ResetEvents[i], FixedCores ? i : -1);
            }
        }
    }
}
