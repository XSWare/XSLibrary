using System;
using XSLibrary.MultithreadingPatterns.UniquePair.DistributionNodes;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public abstract class UniquePairDistribution<PartType, GlobalDataType> : IDisposable
    {
        protected DistributionPool<PartType, GlobalDataType> m_distributionPool;

        public UniquePairDistribution(DistributionPool<PartType, GlobalDataType> pool)
        {
            m_distributionPool = pool;
        }

        public abstract void Calculate(PartType[] parts, GlobalDataType globalData);

        public abstract void Dispose();
    }
}
