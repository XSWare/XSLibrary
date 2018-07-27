namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public class RRTPairing
    {
        public struct PairIDs
        {
            public int ID1;
            public int ID2;
        }

        public PairIDs[/*stepID*/][/*pairID*/] PairMatrix { get; private set; }

        public int StepCount { get; private set; }
        public int PairCount { get; private set; }
        private int ElementCount { get; set; }

        public RRTPairing()
        {
            ElementCount = -1;
        }

        public void GenerateMatrix(int elementCount)
        {
            ElementCount = elementCount;
            PairCount = ElementCount / 2; 
            StepCount = ElementCount - 1;

            PairMatrix = new PairIDs[StepCount][];

            int[] RRTStep = CreateBaseArray();

            for (int step = 0; step < StepCount; step++)
            {
                PairMatrix[step] = new PairIDs[PairCount];
                for (int pairID = 0; pairID < PairCount; pairID++)
                {
                    PairIDs ids;
                    ids.ID1 = RRTStep[pairID];
                    ids.ID2 = RRTStep[ElementCount - 1 - pairID];
                    PairMatrix[step][pairID] = ids;
                }

                if(step + 1 < StepCount)
                    ShiftArray(RRTStep);
            }
        }

        private int[] CreateBaseArray()
        {
            int[] baseIDs = new int[ElementCount];

            for (int i = 0; i < ElementCount; i++)
            {
                baseIDs[i] = i;
            }

            return baseIDs;
        }

        private void ShiftArray(int[] array)
        {
            for (int i = 1; i < ElementCount; i++)
            {
                array[i] = CircleInt(array[i] + 1);
                if (array[i] == 0)
                    array[i]++;
            }
        }

        private int CircleInt(int value)
        {
            int cap = ElementCount;

            if (value < 0)
                return CircleInt(value + cap);

            if (value >= cap)
                return CircleInt(value - cap);

            return value;
        }
    }
}
