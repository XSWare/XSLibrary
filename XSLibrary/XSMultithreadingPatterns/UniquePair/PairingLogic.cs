namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    class PairingLogic
    {
        public int ThreadCount { get; private set; }
        public int StepCount { get; private set; }
        public int StackCount { get; private set; }
        public StackIDs[][] PairMatrix { get; private set; }

        public PairingLogic(int threadCount)
        {
            InitializeStatics(threadCount);
            InitializeMatrix();
        }

        private void InitializeStatics(int threadCount)
        {
            ThreadCount = threadCount;
            StepCount = (2 * ThreadCount) - 1;
            StackCount = 2 * ThreadCount;
        }

        private void InitializeMatrix()
        {
            PairMatrix = new StackIDs[StepCount][];

            int[] IDs = CreateBaseArray();

            for (int step = 0; step < StepCount; step++)
            {
                PairMatrix[step] = new StackIDs[ThreadCount];
                for (int threadID = 0; threadID < ThreadCount; threadID++)
                {
                    StackIDs ids;
                    ids.ID1 = IDs[threadID];
                    ids.ID2 = IDs[StackCount - 1 - threadID];
                    PairMatrix[step][threadID] = ids;
                }

                if(step + 1 < StepCount)
                    ShiftArray(IDs);
            }
        }

        private int[] CreateShiftedArray(int step)
        {
            int[] IDs = CreateBaseArray();

            for (int i = 0; i < step; i++)
            {
                ShiftArray(IDs);
            }

            return IDs;
        }

        private int[] CreateBaseArray()
        {
            int[] baseIDs = new int[StackCount];

            for (int i = 0; i < StackCount; i++)
            {
                baseIDs[i] = i;
            }

            return baseIDs;
        }

        private void ShiftArray(int[] array, int times)
        {
            for (int i = 0; i < times; i++)
                ShiftArray(array);
        }


        private void ShiftArray(int[] array)
        {
            for (int i = 1; i < StackCount; i++)
            {
                array[i] = CircleInt(array[i] + 1);
                if (array[i] == 0)
                    array[i]++;
            }
        }

        private int CircleInt(int value)
        {
            int cap = ThreadCount * 2;

            if (value < 0)
                return CircleInt(value + cap);

            if (value >= cap)
                return CircleInt(value - cap);

            return value;
        }
    }
}
