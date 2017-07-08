namespace XSLibrary.Cryptography.BitManipulation
{
    public partial class Bits 
    {
        class BitPosition
        {
            public int IntIndex { get; private set; }
            public int BitIndex { get; private set; }
            public int RealBitIndex { get; private set; }

            public BitPosition(int realbitIndex)
            {
                RealBitIndex = realbitIndex;
                CalculatePositions();
            }

            private void CalculatePositions()
            {
                IntIndex = GetIntIndex(RealBitIndex);
                BitIndex = GetBitIndex(RealBitIndex);
            }

            public static implicit operator BitPosition(int value)
            {
                return new BitPosition(value);
            }

            public static BitPosition operator+(BitPosition position, int value)
            {
                return new BitPosition(position.RealBitIndex + value);
            }

            public static BitPosition operator ++(BitPosition position)
            {
                return new BitPosition(position.RealBitIndex + 1);
            }

            public static bool operator ==(BitPosition position, int value)
            {
                return position.RealBitIndex == value;
            }

            public static bool operator !=(BitPosition position, int value)
            {
                return !(position.RealBitIndex == value);
            }

            public static bool operator <(BitPosition position, int value)
            {
                return position.RealBitIndex < value;
            }

            public static bool operator >(BitPosition position, int value)
            {
                return position.RealBitIndex > value;
            }

            public static BitPosition GetPosition(int realbitIndex)
            {
                return new BitPosition(realbitIndex);
            }

            public static int GetIntIndex(int realbitIndex)
            {
                return realbitIndex / IntegerBitCount;
            }

            public static int GetBitIndex(int realbitIndex)
            {
                return realbitIndex % IntegerBitCount;
            }

            public override string ToString()
            {
                return "Array index: " + IntIndex + " - Bit: " + BitIndex; 
            }
        }
    }
}
