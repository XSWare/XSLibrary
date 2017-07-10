using System;

namespace XSLibrary.MultithreadingPatterns.UniquePair.DistributionNodes
{
    public abstract partial class DistributionPool<PartType, GlobalDataType> : IDisposable
    {
        public delegate void NodesChangedHandler(object sender, EventArgs e);
        public event NodesChangedHandler OnNodesChanged;

        public abstract int NodeCount { get; }

        public abstract void DistributeCalculation(int nodeIndex, CalculationPair<PartType, GlobalDataType> calculationPair);

        public abstract void Synchronize();

        public abstract void Dispose();
    }
}
