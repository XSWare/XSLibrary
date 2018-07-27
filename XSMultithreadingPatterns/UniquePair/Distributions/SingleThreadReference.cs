namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public class SingleThreadReference<PartType, GlobalDataType> : StandaloneDistribution<PartType, GlobalDataType>
    {
        public SingleThreadReference() : base(1)
        {
        }

        public override void Calculate(PartType[] elements, GlobalDataType globalData)
        {
            for (int i = 0; i < elements.Length - 1; i++)
            {
                for (int j = i + 1; j < elements.Length; j++)
                {
                    CalculationFunction(elements[i], elements[j], globalData);
                }
            }
        }
    }
}