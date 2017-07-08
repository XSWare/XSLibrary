using System;
using System.Runtime.InteropServices;
using System.Threading;

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

    public partial class UniquePairThreading<PartType,GlobalDataType> : IDisposable
    {
        public delegate void PairCalculationFunction(PartType part1, PartType part2, GlobalDataType globalData);

        public bool FixedCores { get; private set; }

        PairingLogic PairLogic { get; set; }
        public int ThreadCount { get { return PairLogic.ThreadCount; } }
        public int StepCount { get { return PairLogic.StepCount; } }
        int StackCount { get { return PairLogic.StackCount; } }

        PartType[][] Stacks { get; set; }
        GlobalDataType GlobalData { get; set; }
        PairCalculationFunction PairFunction { get; set; }
        ActorPool Pool { get; set; }


        public UniquePairThreading(int threadCount) : this(threadCount, false) { }
        public UniquePairThreading(int threadCount, bool fixedCores)
        {
            FixedCores = fixedCores;
            Initialize(threadCount);
        }

        private void Initialize(int threadCount)
        {
            PairLogic = new PairingLogic(threadCount); // needs to be initialized first so all the variables used are intiialized as well

            Stacks = new PartType[StackCount][];
            Pool = new ActorPool(ThreadCount, FixedCores);
        }

        public void Calculate(PartType[] parts, GlobalDataType globalData, PairCalculationFunction pairFunction)
        {
            CreateStacks(parts);
            GlobalData = globalData;
            PairFunction = pairFunction;

            for (int i = 0; i < StepCount; i++)
            {
                CalculateStep(i);
            }
        }

        public void Dispose()
        {
            Pool.Close(true);
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
            int subThreadCount = PairLogic.ThreadCount;

            for (int i = 0; i < subThreadCount; i++)
            {
                Pool.SendMessage(CreateCalculationPair(i, step), i);
                //ThreadPool.QueueUserWorkItem(CreateCalculationPair(i, step).Calculate, finishEvents[i]);
            }

            //CreateCalculationPair(0, step).Calculate();

            Pool.JoinThreads(subThreadCount);
        }

        private CalculationPair CreateCalculationPair(int threadID, int step)
        {
            //int stackID1 = CircleInt(threadID + step);
            //int stackID2 = GetCalculationPartner(threadID, step, step >= ThreadCount);

            CalculationPair data = new CalculationPair(
                Stacks[PairLogic.PairMatrix[step][threadID].ID1],
                Stacks[PairLogic.PairMatrix[step][threadID].ID2],
                GlobalData,
                PairFunction
                );

            data.Step = step;
            data.ThreadID = threadID;
            data.StackID = PairLogic.PairMatrix[step][threadID];

            if (step == 0)
                data.CalculateInternally = true;

            return data;
        }
    }
}
