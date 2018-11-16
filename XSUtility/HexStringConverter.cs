using System;

namespace XSLibrary.Utility
{
    public class HexStringConverter
    {
        public static byte[] ToBytes(string hexString)
        {
            int NumberChars = hexString.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return bytes;
        }

        public static string ToString(byte[] hexBytes)
        {
            return BitConverter.ToString(hexBytes).Replace("-", "");
        }
    }
}
