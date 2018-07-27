using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace XSLibrary.Utility
{
    public class HexStringConverter
    {
        public static byte[] ToBytes(string hexString)
        {
            return SoapHexBinary.Parse(hexString).Value;
        }

        public static string ToString(byte[] hexBytes)
        {
            return new SoapHexBinary(hexBytes).ToString();
        }
    }
}
