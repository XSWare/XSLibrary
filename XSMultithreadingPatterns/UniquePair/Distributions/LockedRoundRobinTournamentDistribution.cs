namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public class LockedRRTDistribution<PartType, GlobalDataType> : LockingDistribution<PartType, GlobalDataType>
    {
        RRTPairing PairLogic { get; set; }

        int CurrentElementCount { get; set; }
        bool m_even;

        public LockedRRTDistribution(int coreCount) : base(coreCount)
        {
            PairLogic = new RRTPairing();

            CurrentElementCount = -1;
        }

        public override void Calculate(PartType[] elements, GlobalDataType globalData)
        {
            if (CurrentElementCount != elements.Length)
            {
                m_even = elements.Length % 2 == 0;

                if (m_even)
                    PairLogic.GenerateMatrix(elements.Length);
                else
                    PairLogic.GenerateMatrix(elements.Length + 1);
            }

            CurrentElementCount = elements.Length;

            base.Calculate(elements, globalData);
        }

        protected override void Distribute(int coreID)
        {
            for (int step = 0; step < PairLogic.StepCount; step++)
            {
                for (int pair = coreID; pair < PairLogic.PairCount; pair += CoreCount)
                {
                    int id1 = PairLogic.PairMatrix[step][pair].ID1;
                    int id2 = PairLogic.PairMatrix[step][pair].ID2;

                    if (!m_even && (id1 == CurrentElementCount || id2 == CurrentElementCount))
                        continue;

                    CalculatePair(id1, id2);
                }
            }
        }
    }
}