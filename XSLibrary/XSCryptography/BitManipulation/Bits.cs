using System;

namespace XSLibrary.Cryptography.BitManipulation
{
    /// <summary>
    /// Dynamically self-resizing chain of bits.
    /// </summary>
    public partial class Bits
    {
        public static bool DisplayAsHex { get; set; } = true;
        public static int FixedStringSize { get; set; } = -1;

        uint[] m_internalArray;

        public int Length { get { return GetBitLength(m_internalArray); } }
        public int Capacity { get { return GetBitsFromInts(m_internalArray.Length); } }

        const int ByteToBitMultiplier = 8;
        const int IntToByteMultiplier = 4;

        static int IntegerBitCount { get { return 4 * ByteToBitMultiplier; } }
        static int IntegerByteCount { get { return IntegerBitCount / ByteToBitMultiplier; } }

        static int LongBitCount { get { return IntegerBitCount * 2; } }
        static int LongByteCount { get { return IntegerByteCount * 2; } }

        static string Zero = "0";
        static string One = "1";

        public Bits()
        {
            ResetArray();
        }

        public Bits(byte byteValue)
        {
            Resize(GetBitsFromBytes(1));
            m_internalArray[0] = byteValue;
        }

        public Bits(byte[] value)
        {
            int neededLength = GetNeededArrayLength(GetBitsFromBytes(value.Length));
            Resize(GetBitsFromInts(neededLength));

            int i;
            for (i = 0; i < value.Length / IntToByteMultiplier; i++)
            {
                m_internalArray[i] = BitConverter.ToUInt32(value, i * IntToByteMultiplier);
            }

            if (i < neededLength)
            {
                uint val = 0;
                for (int j = 0; j < value.Length; j++)
                    val += value[j];

                m_internalArray[neededLength - 1] = val;
            }
        }

        public Bits(uint[] value)
        {
            m_internalArray = CopyArray(value);
        }

        public Bits(int value)
        {
            Resize(IntegerBitCount);
            m_internalArray[0] = (uint)value;
        }

        public Bits(uint value)
        {
            Resize(IntegerBitCount);
            m_internalArray[0] = value;
        }

        public Bits(long value)
        {
            Resize(LongBitCount);
            m_internalArray[0] = (uint)value;
            m_internalArray[1] = (uint)(value >> IntegerBitCount);
        }

        public Bits(ulong value)
        {
            Resize(LongBitCount);
            m_internalArray[0] = (uint)value;
            m_internalArray[1] = (uint)(value >> IntegerBitCount);
        }

        /// <summary>
        /// Converts the last 4 bytes to int. Data in higher positions will be ignored.
        /// </summary>
        /// <returns></returns>
        public int ConvertToInt()
        {
            Grow(IntegerBitCount);
            return (int)m_internalArray[0];
        }

        /// <summary>
        /// Converts the last 4 bytes to uint. Data in higher positions will be ignored.
        /// </summary>
        /// <returns></returns>
        public uint ConvertToUInt()
        {
            Grow(IntegerBitCount);
            return m_internalArray[0];
        }

        /// <summary>
        /// Converts the last 8 bytes to long. Data in higher positions will be ignored.
        /// </summary>
        /// <returns></returns>
        public long ConvertToLong()
        {
            Grow(LongBitCount);
            return ((long)(m_internalArray[1]) << IntegerBitCount) + (m_internalArray[0]);
        }

        /// <summary>
        /// Converts the last 8 bytes to ulong. Data in higher positions will be ignored.
        /// </summary>
        /// <returns></returns>
        public ulong ConvertToULong()
        {
            Grow(LongBitCount);
            return ((ulong)(m_internalArray[1]) << IntegerBitCount) + (m_internalArray[0]);
        }

        public byte[] ConvertToByteArray()
        {
            byte[] byteArray = new byte[m_internalArray.Length * IntToByteMultiplier];
            for (int i = 0; i < m_internalArray.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(m_internalArray[i]), 0, byteArray, i * IntToByteMultiplier, IntToByteMultiplier);
            }
            return byteArray;
        }

        public string ToString(int digitCount)
        {
            if (DisplayAsHex)
                return ToHexString(digitCount);
            else
                return ToBinaryString(digitCount);
        }

        public override string ToString()
        {
            if(DisplayAsHex)
                return ToHexString();
            else
                return ToBinaryString();
        }

        public string ToBinaryString()
        {
            if (FixedStringSize > -1)
                return ToBinaryString(FixedStringSize);

            return ToBinaryString(Length);
        }

        public string ToHexString()
        {
            if (FixedStringSize > -1)
                return ToHexString(FixedStringSize);

            return ToHexString((Length / 4) + ((Length % 4 > 0) ? 1 : 0));
        }

        public string ToHexString(int fixedDigits)
        {
            Grow(fixedDigits * 4);

            int entryCount = (fixedDigits / 8) + ((fixedDigits % 8 > 0) ? 1 : 0);

            string hex = "";
            for (int i = entryCount - 1; i >= 0; i--)
            {
                string single = m_internalArray[i].ToString("x8");
                hex += single;
            }
            if (hex.Length <= 0)
                return Zero;

            return SetStringFixedSize(hex, fixedDigits);
        }

        public string ToBinaryString(int fixedDigits)
        {
            if (FirstBitNotZeroIndex(m_internalArray) == -1)
            {
                if (fixedDigits == 0)
                    return Zero;
                else
                    return ZeroPadding("", fixedDigits);
            }

            int spaceModulo = fixedDigits % ByteToBitMultiplier;
            int spaceCount = fixedDigits / ByteToBitMultiplier - (fixedDigits % ByteToBitMultiplier == 0 ? 1 : 0);

            string result = "";
            for (int bitIndex = fixedDigits - 1; bitIndex >= 0; bitIndex--)
            {
                bool lastOrFirst = result.Length <= 0 || bitIndex <= 0;
                if ((fixedDigits - (bitIndex + 1)) % ByteToBitMultiplier == spaceModulo && !lastOrFirst)
                    result += " ";

                BitPosition bitPosition = new BitPosition(bitIndex);

                if (bitPosition.IntIndex >= m_internalArray.Length)
                    result += Zero;
                else if (bitIndex < 0)
                    break;
                else
                    result += IsBitSet(bitIndex) ? One : Zero;
            }

            return SetStringFixedSize(result , fixedDigits + spaceCount);
        }

        string SetStringFixedSize(string str, int size)
        {
            return (ZeroPadding(TrimLeft(str, size), size));
        }

        string TrimLeft(string toCut, int size)
        {
            if (size <= 0)
                return "";

            if (size >= toCut.Length)
                return toCut;

            return toCut.Substring(toCut.Length - size, size);
        }

        string ZeroPadding(string toPad, int size)
        {
            string preZeros = "";
            for (int i = toPad.Length; i < size; i ++)
            {
                preZeros += Zero;
            }
            return preZeros + toPad;
        }
    }
}
