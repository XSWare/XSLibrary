using System;
using System.Runtime.InteropServices;
using XSLibrary.MultithreadingPatterns.UniquePair.DistributionNodes;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    static class DDLExports
    {
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
    }

    struct StackIDs
    {
        public int ID1;
        public int ID2;
    }

    public partial class RoundRobinTournamentDistribution<PartType, GlobalDataType> : UniquePairDistribution<PartType, GlobalDataType>
    {
        PairingLogic PairLogic { get; set; }
        public int ThreadCount { get { return PairLogic.ThreadCount; } }
        public int StepCount { get { return PairLogic.StepCount; } }
        int StackCount { get { return PairLogic.StackCount; } }

        PartType[][] Stacks { get; set; }
        GlobalDataType GlobalData { get; set; }

        public RoundRobinTournamentDistribution(DistributionPool<PartType, GlobalDataType> pool) : base(pool)
        {
            PairLogic = new PairingLogic(pool.NodeCount); // needs to be initialized first so all the variables used are intiialized as well

            Stacks = new PartType[StackCount][];
        }

        public override void Calculate(PartType[] parts, GlobalDataType globalData)
        {
            CreateStacks(parts);
            GlobalData = globalData;

            for (int i = 0; i < StepCount; i++)
            {
                CalculateStep(i);
            }
        }

        public override void Dispose()
        {
            m_distributionPool.Dispose();
        }

        private void CreateStacks(PartType[] parts)
        {
            int stackSize = parts.Length / StackCount;
            int leftover = parts.Length % StackCount;

            for (int i = 0; i < StackCount; i++)
            {
                // as there might be numbers of parts which are not divideable cleanly
                // the leftovers get added one by one to the first few stacks

                // e.g. 6 parts divided by 4 stacks would mean Stacks[0] and Stacks[1] would hold 2 values 
                // while Stacks[2] and Stacks[3] hold only one
                if (i < leftover)
                {
                    Stacks[i] = new PartType[stackSize + 1];
                    Array.Copy(parts, i * stackSize + i, Stacks[i], 0, stackSize + 1);
                }
                else
                {
                    Stacks[i] = new PartType[stackSize];
                    Array.Copy(parts, i * stackSize + leftover, Stacks[i], 0, stackSize);
                }
            }
        }

        private void CalculateStep(int step)
        {
            for (int i = 0; i < PairLogic.ThreadCount; i++)
            {
                m_distributionPool.DistributeCalculation(i, CreateCalculationPair(i, step));
            }

            m_distributionPool.Synchronize();
        }

        private CalculationPair<PartType, GlobalDataType> CreateCalculationPair(int threadID, int step)
        {
            //int stackID1 = CircleInt(threadID + step);
            //int stackID2 = GetCalculationPartner(threadID, step, step >= ThreadCount);

            CalculationPair<PartType, GlobalDataType> data = new CalculationPair<PartType, GlobalDataType>(
                Stacks[PairLogic.PairMatrix[step][threadID].ID1],
                Stacks[PairLogic.PairMatrix[step][threadID].ID2],
                GlobalData,
                step == 0
                );

            //data.Step = step;
            //data.ThreadID = threadID;
            //data.StackID = PairLogic.PairMatrix[step][threadID];

            return data;
        }
    }
}
