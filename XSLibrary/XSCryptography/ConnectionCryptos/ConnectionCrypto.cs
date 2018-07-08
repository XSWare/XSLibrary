using System;
using System.Net;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    public abstract class IConnectionCrypto
    {
        public delegate bool ReceiveCall(out byte[] data, out IPEndPoint source);

        bool Active { get; set; }

        public IConnectionCrypto(bool active)
        {
            Active = active;
        }

        public bool Handshake(Action<byte[]> Send, ReceiveCall Receive)
        {
            if (Active)
                return HandshakeActive(Send, Receive);
            else
                return HandshakePassive(Send, Receive);
        }

        protected abstract bool HandshakeActive(Action<byte[]> Send, ReceiveCall Receive);
        protected abstract bool HandshakePassive(Action<byte[]> Send, ReceiveCall Receive);

        public abstract byte[] EncryptData(byte[] data);
        public abstract byte[] DecryptData(byte[] data);
    }
}
