//#define CONSOLE

using System;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public partial class UniquePairThreading<PartType, GlobalDataType>
    {
        class CalculationPair : IDisposable
        {
            PartType[] m_stack1;
            PartType[] m_stack2;
            GlobalDataType m_globalData;
            PairCalculationFunction m_pairFunction;
            public int ThreadID;
            public int Step;
            public StackIDs StackID;

            public bool CalculateInternally;

            public CalculationPair(PartType[] part1, PartType[] part2, GlobalDataType globalData, PairCalculationFunction function)
            {
                m_stack1 = part1;
                m_stack2 = part2;
                m_globalData = globalData;
                m_pairFunction = function;

                CalculateInternally = false;
            }

            public void Calculate()
            {
#if CONSOLE
                Console.Out.WriteLine("Calculate - Step {0} - ThreadID {1} - Stacks<{2},{3}>", Step, ThreadID, StackID.ID1, StackID.ID2);
#endif

                if (CalculateInternally)
                {
                    CalculateStackInternal(m_stack1);
                    CalculateStackInternal(m_stack2);
                }

                CalculateStackPair();             
            }

            private void CalculateStackInternal(PartType[] stack)
            {
                if (stack.Length < 2)
                    return;

                for (int i = 0; i < stack.Length - 1; i++)
                {
                    for (int j = i + 1; j < stack.Length; j++)
                        m_pairFunction(stack[i], stack[j], m_globalData);
                }
            }

            private void CalculateStackPair()
            {
                foreach (PartType part1 in m_stack1)
                {
                    foreach (PartType part2 in m_stack2)
                        m_pairFunction(part1, part2, m_globalData);
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected void Dispose(bool disposing)
            {
#if CONSOLE
                Console.Out.WriteLine("Dispose - Step {0} - ThreadID {1} - Stacks<{2},{3}>", Step, ThreadID, StackID.ID1, StackID.ID2);
#endif

                if (disposing)
                {
                    m_stack1 = null;
                    m_stack2 = null;

                    m_pairFunction = null;
                }

                
            }
        }
    }
}
