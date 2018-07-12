using System;
using XSLibrary.Cryptography.ConnectionCryptos;

namespace XSLibrary.Network.Connections
{
    public abstract partial class IConnection
    {
        public bool InitializeCrypto(IConnectionCrypto crypto)
        {
            if (!m_handshakeLock.Execute(() => ExecuteCryptoHandshake(crypto)))
            {
                HandleHandshakeFailure();
                return false;
            }

            return true;
        }

        private bool ExecuteCryptoHandshake(IConnectionCrypto crypto)
        {
            int previousTimeout = ConnectionSocket.ReceiveTimeout;
            ConnectionSocket.ReceiveTimeout = HandshakeTimeout;

            try
            {
                if (Receiving)
                {
                    Logger.Log("Crypto cannot be initiated after receive loop was started!");
                    return false;
                }

                ExecutePreReceiveActions();

                if (!crypto.Handshake((data) => SafeSend(() => SendSpecialized(data)), SafeReceive))
                    return false;

                Crypto = crypto;
                Logger.Log("Crypto handshake successful.");
                return true;
            }
            catch (Exception ex) { return false; }
            finally { ConnectionSocket.ReceiveTimeout = previousTimeout; }
        }

        private void HandleHandshakeFailure()
        {
            Logger.Log("Crypto handshake failed!");
            Disconnect();
        }
    }
}
