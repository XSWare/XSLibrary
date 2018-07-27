using System;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public partial class SynchronizedRRTDistribution<ElementType, GlobalDataType> : CorePoolDistribution<ElementType, GlobalDataType>
    {
        RRTPairing PairLogic { get; set; }

        int ElementCount { get; set; }
        int UsableCoreCount { get; set; }

        ElementType[][] Stacks { get; set; }
        GlobalDataType GlobalData { get; set; }

        public SynchronizedRRTDistribution(CorePool<ElementType, GlobalDataType> pool) : base(pool)
        {
            PairLogic = new RRTPairing();
        }

        public override void Calculate(ElementType[] elements, GlobalDataType globalData)
        {
            ElementCount = elements.Length;

            int previouslyUsableCores = UsableCoreCount;
            UsableCoreCount = CalculateUsableCoreCount(ElementCount);

            CreateStacks(elements);
            GlobalData = globalData;

            if (previouslyUsableCores != UsableCoreCount)
                PairLogic.GenerateMatrix(UsableCoreCount * 2);

            for (int step = 0; step < PairLogic.StepCount; step++)
            {
                CalculateStep(step);
            }
        }

        private int CalculateUsableCoreCount(int elementCount)
        {
            return Math.Min(CoreCount, elementCount / 2);
        }

        private void CreateStacks(ElementType[] elements)
        {
            int stackCount = UsableCoreCount * 2;

            Stacks = new ElementType[stackCount][];

            int stackSize = ElementCount / stackCount;
            int leftover = ElementCount % stackCount;

            for (int i = 0; i < stackCount; i++)
            {
                // as there might be numbers of parts which are not divideable cleanly
                // the leftovers get added one by one to the first few stacks

                // e.g. 6 parts divided by 4 stacks would mean Stacks[0] and Stacks[1] would hold 2 values 
                // while Stacks[2] and Stacks[3] hold only one
                if (i < leftover)
                {
                    Stacks[i] = new ElementType[stackSize + 1];
                    Array.Copy(elements, i * stackSize + i, Stacks[i], 0, stackSize + 1);
                }
                else
                {
                    Stacks[i] = new ElementType[stackSize];
                    Array.Copy(elements, i * stackSize + leftover, Stacks[i], 0, stackSize);
                }
            }
        }

        private void CalculateStep(int step)
        {
            for (int i = 0; i < UsableCoreCount; i++)
            {
                CorePool.DistributeCalculation(i, CreateCalculationPair(i, step));
            }

            CorePool.Synchronize();
        }

        private PairingData<ElementType, GlobalDataType> CreateCalculationPair(int coreID, int step)
        {
            return new PairingData<ElementType, GlobalDataType>(
                Stacks[PairLogic.PairMatrix[step][coreID].ID1],
                Stacks[PairLogic.PairMatrix[step][coreID].ID2],
                GlobalData,
                step == 0);
        }
    }
}
