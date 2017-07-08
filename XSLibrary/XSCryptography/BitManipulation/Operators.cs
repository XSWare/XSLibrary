namespace XSLibrary.Cryptography.BitManipulation
{
    public partial class Bits
    {
        public static implicit operator Bits(int value)
        {
            return new Bits(value);
        }

        public static implicit operator int(Bits value)
        {
            return value.ConvertToInt();
        }

        public static implicit operator Bits(uint value)
        {
            return new Bits(value);
        }

        public static implicit operator uint(Bits value)
        {
            return value.ConvertToUInt();
        }

        public static implicit operator Bits(long value)
        {
            return new Bits(value);
        }

        public static implicit operator long(Bits value)
        {
            return value.ConvertToLong();
        }

        public static implicit operator Bits(ulong value)
        {
            return new Bits(value);
        }

        public static implicit operator ulong(Bits value)
        {
            return value.ConvertToULong();
        }

        public static implicit operator Bits(byte value)
        {
            return new Bits(value);
        }

        public static implicit operator Bits(byte[] value)
        {
            return new Bits(value);
        }

        public static implicit operator Bits(uint[] value)
        {
            return new Bits(value);
        }

        public static implicit operator byte[](Bits value)
        {
            return value.ConvertToByteArray();
        }

        public static implicit operator uint[] (Bits value)
        {
            return value.CopyInternalArray();
        }

        public static bool operator ==(Bits bits, Bits other)
        {
            return ArraysEqual(bits.m_internalArray, other.m_internalArray);
        }

        public static bool operator !=(Bits bits, Bits other)
        {
            return !(bits == other);
        }

        public static bool operator ==(Bits bits, byte[] other)
        {
            return ArraysEqual(bits, new Bits(other));
        }

        public static bool operator !=(Bits bits, byte[] other)
        {
            return !(bits == other);
        }

        public static bool operator <(Bits bits, Bits other)
        {
            return FirstBitNotZeroIndex(bits.m_internalArray) < FirstBitNotZeroIndex(other.m_internalArray);
        }

        public static bool operator >(Bits bits, Bits other)
        {
            return FirstBitNotZeroIndex(bits.m_internalArray) > FirstBitNotZeroIndex(other.m_internalArray);
        }

        public static Bits operator +(Bits bits, Bits other)
        {
            uint[] result = bits;
            Add(ref result, other.m_internalArray);
            return result;
        }

        public static Bits operator ~(Bits bits)
        {
            uint[] result = bits;
            NOT(ref result);
            return new Bits(result);
        }

        public static Bits operator &(Bits bits, Bits other)
        {
            uint[] result = bits;
            AND(ref result, other.m_internalArray);
            return new Bits(result);
        }

        public static Bits operator |(Bits bits, Bits other)
        {
            uint[] result = bits;
            OR(ref result, other.m_internalArray);
            return new Bits(result);
        }

        public static Bits operator ^(Bits bits, Bits other)
        {
            uint[] result = bits;
            XOR(ref result, other.m_internalArray);
            return new Bits(result);
        }

        public static Bits operator <<(Bits bits, int value)
        {
            uint[] result = bits;
            ShiftLeft(ref result, value);
            return result;
        }

        public static Bits operator >>(Bits bits, int value)
        {
            uint[] result = bits;
            ShiftRight(ref result, value);
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Bits other = (Bits)obj;
            return ArraysEqual(m_internalArray, other.m_internalArray);
        }

        public override int GetHashCode()
        {
            return m_internalArray.GetHashCode();
        }
    }
}
