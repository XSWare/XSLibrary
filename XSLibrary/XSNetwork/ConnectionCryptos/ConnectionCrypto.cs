using System;
using System.Net;

namespace XSLibrary.Network.ConnectionCryptos
{
    public abstract class IConnectionCrypto
    {
        public delegate bool ReceiveCall(out byte[] data, out IPEndPoint source);

        bool Active { get; set; }

        public abstract bool Handshake(Action<byte[]> Send, ReceiveCall Receive);

        public abstract byte[] EncryptData(byte[] data);
        public abstract byte[] DecryptData(byte[] data);
    }
}
