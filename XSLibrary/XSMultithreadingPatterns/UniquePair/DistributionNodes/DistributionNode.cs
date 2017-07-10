namespace XSLibrary.MultithreadingPatterns.UniquePair.DistributionNodes
{
    public partial class DistributionPool<PartType, GlobalDataType>
    {
        public abstract class DistributionNode
        {
            public abstract void CalculateStacks(CalculationPair<PartType, GlobalDataType> calculationPair);
        }
    }
}