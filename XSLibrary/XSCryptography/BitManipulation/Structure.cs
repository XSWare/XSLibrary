using System;

namespace XSLibrary.Cryptography.BitManipulation
{
    public partial class Bits
    {
        /// <summary>
        /// Creates a new array a dynamic size greater or equal than the requested size.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private static uint[] CreateDynamicallySizedArray(int bitCount)
        {
            return new uint[FindPowerTwo(GetNeededArrayLength(bitCount))];
        }

        /// <summary>
        /// Creates a new array with the requested size.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private static uint[] CreateStaticallySizedArray(int bitCount)
        {
            return new uint[(GetNeededArrayLength(bitCount))];
        }

        /// <summary>
        /// Grows the internal array dynamically for the amount of requested bits without deleting any data.
        /// </summary>
        /// <param name="size"></param>
        private void Grow(int bitCount)
        {
            GrowDynamically(ref m_internalArray, bitCount);
        }

        /// <summary>
        /// Grows the array dynamically for the amount of requested bits without deleting any data.
        /// </summary>
        /// <param name="size"></param>
        private static void GrowDynamically(ref uint[] array, int bitCount)
        {
            int neededSize = GetNeededArrayLength(bitCount);
            if (array.Length >= neededSize)
                return;

            uint[] previous = array;
            ResizeDynamically(ref array, bitCount);
            Array.Copy(previous, array, Math.Min(array.Length, previous.Length));
        }

        /// <summary>
        /// Grows the array exactly for the amount of requested bits without deleting any data.
        /// </summary>
        /// <param name="size"></param>
        private static void GrowStatically(ref uint[] array, int bitCount)
        {
            int neededBytes = GetNeededArrayLength(bitCount);
            if (array.Length >= neededBytes)
                return;

            uint[] previous = array;
            ResizeStatically(ref array, bitCount);
            Array.Copy(previous, array, Math.Min(array.Length, previous.Length));
        }

        /// <summary>
        /// Resizes the internal array to fit the amount of requested bits. Releases memory and deletes data. 
        /// </summary>
        /// <param name="bitCount"></param>
        private void Resize(int bitCount)
        {
            ResizeDynamically(ref m_internalArray, bitCount);
        }

        /// <summary>
        /// Resizes the array to dynamically fit the amount of requested bits. Releases memory and deletes data. 
        /// </summary>
        /// <param name="bitCount"></param>
        private static void ResizeDynamically(ref uint[] array, int bitCount)
        {
            array = CreateDynamicallySizedArray(bitCount);
        }

        /// <summary>
        /// Resizes the array to fit the amount of requested bytes. Releases memory and deletes data. 
        /// </summary>
        /// <param name="bitCount"></param>
        private static void ResizeStatically(ref uint[] array, int bitCount)
        {
            array = CreateStaticallySizedArray(bitCount);
        }

        /// <summary>
        /// Finds the next power of 2 value (e.g. 2, 4, 32, 256) which is greater or equal than the parameter.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static int FindPowerTwo(int number)
        {
            int shift = 0;

            while ((1 << shift) < number)
            {
                shift++;
            }

            return 1 << shift;
        }

        /// <summary>
        /// Sets the value to zero without resizing the array.
        /// </summary>
        private void SetZero()
        {
            for (int i = 0; i < m_internalArray.Length; i++)
                m_internalArray[i] = 0;
        }

        /// <summary>
        /// Resets the array to default state and deletes all data.
        /// </summary>
        private void ResetArray()
        {
            m_internalArray = new uint[0];
        }

        /// <summary>
        /// Creates a copy of the internal array and returns a reference to it.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private uint[] CopyInternalArray()
        {
            return CopyArray(m_internalArray);
        }

        /// <summary>
        /// Creates a copy of the array and returns a reference to it.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private uint[] CopyArray(uint[] array)
        {
            uint[] copy = new uint[array.Length];
            Array.Copy(array, copy, array.Length);
            return copy;
        }

        /// <summary>
        /// Calculates the array size needed to fit the amount of bits declared in the parameter.
        /// </summary>
        /// <param name="bitCount"></param>
        /// <returns></returns>
        private static int GetNeededArrayLength(int bitCount)
        {
            return (bitCount / IntegerBitCount) + (bitCount % IntegerBitCount == 0 ? 0 : 1);
        }

        private static int GetBitsFromBytes(int byteCount)
        {
            return byteCount * ByteToBitMultiplier;
        }

        private static int GetBitsFromInts(int intCount)
        {
            return intCount * IntegerBitCount;
        }
    }
}
