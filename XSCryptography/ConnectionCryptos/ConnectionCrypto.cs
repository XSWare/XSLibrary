using System.Net;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    public abstract class IConnectionCrypto
    {
        public delegate bool SendCall(byte[] data);
        public delegate bool ReceiveCall(out byte[] data, out EndPoint source);

        bool Active { get; set; }

        public IConnectionCrypto(bool active)
        {
            Active = active;
        }

        public bool Handshake(SendCall Send, ReceiveCall Receive)
        {
            if (Active)
                return HandshakeActive(Send, Receive);
            else
                return HandshakePassive(Send, Receive);
        }

        protected abstract bool HandshakeActive(SendCall Send, ReceiveCall Receive);
        protected abstract bool HandshakePassive(SendCall Send, ReceiveCall Receive);

        public abstract byte[] EncryptData(byte[] data);
        public abstract byte[] DecryptData(byte[] data);
    }
}
