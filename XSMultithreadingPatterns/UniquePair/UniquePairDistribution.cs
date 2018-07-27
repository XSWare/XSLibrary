using System;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public abstract class UniquePairDistribution<ElementType, GlobalDataType> : IDisposable
    {
        public delegate void PairCalculationFunction(ElementType element1, ElementType element2, GlobalDataType globalData);

        public abstract int CoreCount { get; }

        public abstract void SetCalculationFunction(PairCalculationFunction function);
        public abstract void Calculate(ElementType[] elements, GlobalDataType globalData);

        public virtual void Dispose()
        {
        }
    }
}