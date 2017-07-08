using System;

namespace XSLibrary.Cryptography.BitManipulation
{
    public partial class Bits
    {
        public void SetBit(int bitIndex)
        {
            SetBit(ref m_internalArray, bitIndex);
        }

        public static void SetBit(ref uint[] array, int bitIndex)
        {
            BitPosition position = bitIndex;
            GrowDynamically(ref array, bitIndex + 1);
            uint mask = (uint)1 << position.BitIndex;
            array[position.IntIndex] |= mask;
        }

        public void ResetBit(int bitIndex)
        {
            ResetBit(ref m_internalArray, bitIndex);
        }

        public static void ResetBit(ref uint[] array, int bitIndex)
        {
            BitPosition position = bitIndex;
            GrowDynamically(ref array, bitIndex + 1);
            uint mask = (uint)((1 << position.BitIndex) ^ 0xFF);
            array[position.IntIndex] &= mask;
        }

        /// <summary>
        /// Returns true if the bit with the zero based index "bitIndex" is set.
        /// </summary>
        /// <param name="bitIndex"></param>
        /// <returns></returns>
        public bool IsBitSet(int bitIndex)
        {
            return IsBitSet(m_internalArray, BitPosition.GetBitIndex(bitIndex), BitPosition.GetIntIndex(bitIndex));
        }

        public static bool IsBitInArraySet(uint[] array, int bitIndex)
        {
            return IsBitSet(array, BitPosition.GetBitIndex(bitIndex), BitPosition.GetIntIndex(bitIndex));
        }

        private static bool IsBitSet(uint[] array, int bitPosition, int intPosition)
        {
            if (intPosition >= array.Length)
                return false;

            int mask = 1 << bitPosition;
            return ((mask & array[intPosition]) >> bitPosition) > 0;
        }

        private static int GetBitLength(uint[] array)
        {
            return FirstBitNotZeroIndex(array) + 1;
        }

        private static int FirstBitNotZeroIndex(uint[] array)
        {
            for (int arrayIndex = array.Length - 1; arrayIndex >= 0; arrayIndex--)
            {
                if (array[arrayIndex] > 0)
                {

                    for (int bitIndex = IntegerBitCount - 1; bitIndex >= 0; bitIndex--)
                    {
                        if (IsBitSet(array, bitIndex, arrayIndex))
                            return (arrayIndex * IntegerBitCount) + bitIndex;
                    }
                }
            }

            return -1;
        }

        public static bool ArraysEqual(uint[] array1, uint[] array2)
        {
            int max = Math.Max(array1.Length, array2.Length);

            for (int i = 0; i < max; i++)
            {
                if(i < array1.Length && i < array2.Length)
                {
                    if(array1[i] != array2[i])
                        return false;
                }
                else if (i >= array1.Length && array2[i] > 0)
                    return false;
                else if (i >= array2.Length && array1[i] > 0)
                    return false;
            }

            return true;
        }

        public static void Add(ref uint[] resultArray, uint[] operatorArray)
        {
            GrowDynamically(ref resultArray, GetBitsFromInts(operatorArray.Length));

            int carry = 0;
            for (int i = 0; i < operatorArray.Length; i++)
            {
                long addResult = resultArray[i] + operatorArray[i] + carry;
                resultArray[i] = (uint)addResult;
                carry = (addResult >> IntegerBitCount) > 0 ? 1 : 0;
            }

            if(carry != 0)
            {
                GrowDynamically(ref resultArray, GetBitsFromInts(resultArray.Length) + 1);
                resultArray[operatorArray.Length] = (uint)carry;
            }
        }

        /// <summary>
        /// Shifts the bits of the array "count" times left.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        public static void ShiftLeft(ref uint[] array, int count)
        {
            int entryShifts = count / IntegerBitCount;
            int bitShifts = count % IntegerBitCount;

            if(entryShifts > 0)
                ShiftEntriesLeft(ref array, entryShifts);

            for (int i = 0; i < bitShifts; i++)
                ShiftLeft(ref array);
        }

        /// <summary>
        /// Shifts compelete entries of the array left.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="steps"></param>
        private static void ShiftEntriesLeft(ref uint[] array, int steps)
        {
            BitPosition position = GetBitLength(array);
            GrowDynamically(ref array, position.RealBitIndex + (steps * IntegerBitCount) + 1);

            for(int i = position.IntIndex; i >= 0 ; i--)
            {
                array[i + steps] = array[i];

                if (i < steps)
                    array[i] = 0;
            }
        }

        /// <summary>
        /// Shifts the array a single bit to the left.
        /// </summary>
        /// <param name="array"></param>
        public static void ShiftLeft(ref uint[] array)
        {
            bool carry = false;
            for (int i = array.Length - 1; i >= 0; i--)
            {
                carry = array[i] >= (uint)1 << (IntegerBitCount - 1);
                if (carry && i == array.Length - 1)
                    GrowDynamically(ref array, GetBitsFromInts(array.Length + 1));

                if (carry)
                    array[i + 1]++;

                array[i] = array[i] << 1;
            }
        }

        /// <summary>
        /// Shifts the bits of the array "count" times right.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        public static void ShiftRight(ref uint[] array, int count)
        {

            int entryShifts = count / IntegerBitCount;
            int bitShifts = count % IntegerBitCount;

            if (entryShifts > 0)
                ShiftEntriesRight(ref array, entryShifts);

            for (int i = 0; i < bitShifts; i++)
                ShiftRight(ref array);
        }

        /// <summary>
        /// Shifts compelete entries of the array right.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="steps"></param>
        private static void ShiftEntriesRight(ref uint[] array, int steps)
        {
            BitPosition position = FirstBitNotZeroIndex(array);

            int newSize = position.IntIndex - steps + 1;

            for (int i = 0; i <= position.IntIndex; i++)
            {
                if (i + steps <= position.IntIndex)
                    array[i] = array[i + steps];
                else 
                    array[i] = 0;
            }
        }

        /// <summary>
        /// Executes the "shift to the right" operation on the array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static void ShiftRight(ref uint[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] % 2 > 0 && i > 0)
                    array[i - 1] |= (uint)1 << (IntegerBitCount - 1);

                array[i] = array[i] >> 1;
            }
        }

        /// <summary>
        /// Executes the "NOT" operation on the array.
        /// </summary>
        /// <param name="resultArray"></param>
        /// <param name="operatorArray"></param>
        public static void NOT(ref uint[] resultArray)
        {
            ArrayEntryOperation(ref resultArray, (uint part) => ~part);
        }

        /// <summary>
        /// Executes the "AND" operation on the array.
        /// </summary>
        /// <param name="resultArray"></param>
        /// <param name="operatorArray"></param>
        public static void AND(ref uint[] resultArray, uint[] operatorArray)
        {
            GrowStatically(ref operatorArray, GetBitsFromInts(resultArray.Length));
            ArrayEntryOperation(ref resultArray, operatorArray, (uint entry1, uint entry2) => entry1 & entry2);
        }

        /// <summary>
        /// Executes the "OR" operation on the array.
        /// </summary>
        /// <param name="resultArray"></param>
        /// <param name="operatorArray"></param>
        public static void OR(ref uint[] resultArray, uint[] operatorArray)
        {
            ArrayEntryOperation(ref resultArray, operatorArray, (uint entry1, uint entry2) => entry1 | entry2);
        }

        /// <summary>
        /// Executes the "XOR" operation on the array.
        /// </summary>
        /// <param name="resultArray"></param>
        /// <param name="operatorArray"></param>
        public static void XOR(ref uint[] resultArray, uint[] operatorArray)
        {
            ArrayEntryOperation(ref resultArray, operatorArray, (uint entry1, uint entry2) => entry1 ^ entry2);
        }

        private static void ArrayEntryOperation(ref uint[] resultArray, Func<uint, uint> operation)
        {
            for (int i = 0; i < resultArray.Length; i++)
            {
                resultArray[i] = operation(resultArray[i]);
            }
        }

        private static void ArrayEntryOperation(ref uint[] resultArray, uint[] operatorArray, Func<uint, uint, uint> operation)
        {
            int size = GetBitsFromInts((Math.Max(resultArray.Length, operatorArray.Length)));
            GrowDynamically(ref resultArray, size);

            for (int i = 0; i < operatorArray.Length; i++)
            {
                resultArray[i] = operation(resultArray[i], operatorArray[i]);
            }
        }
    }
}
