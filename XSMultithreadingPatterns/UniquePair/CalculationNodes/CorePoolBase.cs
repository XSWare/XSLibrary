using System;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public abstract partial class CorePoolBase<PartType, GlobalDataType> : IDisposable
    {
        public delegate void NodesChangedHandler(object sender, EventArgs e);
        public event NodesChangedHandler OnNodesChanged;

        public abstract int CoreCount { get; }

        public abstract void DistributeCalculation(int coreIndex, PairingData<PartType, GlobalDataType> calculationPair);

        public abstract void Synchronize();
        public abstract void Synchronize(int coreIndex);

        public abstract void Dispose();
    }
}