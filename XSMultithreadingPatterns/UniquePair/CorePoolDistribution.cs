namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public abstract class CorePoolDistribution<PartType, GlobalDataType> : UniquePairDistribution<PartType, GlobalDataType>
    {
        protected CorePool<PartType, GlobalDataType> CorePool { get; private set; }
        public sealed override int CoreCount { get { return CorePool.CoreCount; } }

        public CorePoolDistribution(CorePool<PartType, GlobalDataType> pool)
        {
            CorePool = pool;
        }

        public sealed override void SetCalculationFunction(PairCalculationFunction function)
        {
            CorePool.SetCalculationFunction(function);
        }

        public override void Dispose()
        {
            CorePool.Dispose();
        }
    }
}
