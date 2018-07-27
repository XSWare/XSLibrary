namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public partial class CorePoolBase<PartType, GlobalDataType>
    {
        public abstract class CalculationCore
        {
            public abstract void CalculatePairedData(PairingData<PartType, GlobalDataType> calculationPair);
        }
    }
}