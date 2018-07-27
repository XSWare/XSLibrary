using System;

namespace XSLibrary.MultithreadingPatterns.UniquePair
{
    public class SharedMemoryStackCalculation<PartType, GlobalDataType>
    {
        UniquePairDistribution<PartType, GlobalDataType>.PairCalculationFunction m_pairFunction;

        public SharedMemoryStackCalculation()
        {
        }

        public SharedMemoryStackCalculation(UniquePairDistribution<PartType, GlobalDataType>.PairCalculationFunction calculationFunction)
        {
            SetCalculationFunction(calculationFunction);
        }

        public void SetCalculationFunction(UniquePairDistribution<PartType, GlobalDataType>.PairCalculationFunction calculationFunction)
        {
            m_pairFunction = calculationFunction;
        }

        public void Calculate(PairingData<PartType, GlobalDataType> pair)
        {
            if (m_pairFunction == null)
                throw new Exception("Calculation function not initialized.");

            if (pair.CalculateInternally)
            {
                CalculateStackInternal(pair.Stack1, pair.GlobalData);
                CalculateStackInternal(pair.Stack2, pair.GlobalData);
            }

            CalculateStackPair(pair.Stack1, pair.Stack2, pair.GlobalData);
        }

        private void CalculateStackInternal(PartType[] stack, GlobalDataType globalData)
        {
            if (stack.Length < 2)
                return;

            for (int i = 0; i < stack.Length - 1; i++)
            {
                for (int j = i + 1; j < stack.Length; j++)
                    m_pairFunction(stack[i], stack[j], globalData);
            }
        }

        private void CalculateStackPair(PartType[] stack1, PartType[] stack2, GlobalDataType globalData)
        {
            foreach (PartType part1 in stack1)
            {
                foreach (PartType part2 in stack2)
                    m_pairFunction(part1, part2, globalData);
            }
        }
    }
}
