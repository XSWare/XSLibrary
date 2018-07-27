namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public abstract class StandaloneDistribution<PartType, GlobalDataType> : UniquePairDistribution<PartType, GlobalDataType>
    {
        protected PairCalculationFunction CalculationFunction { get; private set; }
        private int m_coreCount;

        public sealed override int CoreCount { get { return m_coreCount; } }

        public StandaloneDistribution(int coreCount)
        {
            m_coreCount = coreCount;
        }

        public sealed override void SetCalculationFunction(PairCalculationFunction function)
        {
            CalculationFunction = function;
        }
    }
}