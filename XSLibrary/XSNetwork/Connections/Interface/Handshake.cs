using System;
using System.Net;
using XSLibrary.Cryptography.ConnectionCryptos;
using XSLibrary.ThreadSafety.Executors;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public abstract partial class IConnection
    {
        private SafeExecutor m_handshakeLock = new SingleThreadExecutor();

        public bool InitializeCrypto(IConnectionCrypto crypto, int timeout = 5000)
        {
            if (!m_handshakeLock.Execute(() => ExecuteCryptoHandshake(crypto, timeout)))
            {
                HandleHandshakeFailure();
                return false;
            }

            return true;
        }

        private bool ExecuteCryptoHandshake(IConnectionCrypto crypto, int timeout)
        {
            try
            {
                if (Receiving)
                {
                    Logger.Log(LogLevel.Error, "Crypto cannot be initiated after receive loop was started!");
                    return false;
                }

                ExecutePreReceiveActions();

                if (!crypto.Handshake(
                    (data) => SafeSend(() => SendSpecialized(data), timeout),  // send
                    (out byte[] data, out EndPoint source) => SafeReceive(out data, out source, timeout))) // receive
                {
                    return false;
                }

                Crypto = crypto;
                Logger.Log(LogLevel.Information, "Crypto handshake successful.");
                return true;
            }
            catch (Exception) { return false; }
        }

        private void HandleHandshakeFailure()
        {
            Logger.Log(LogLevel.Error, "Crypto handshake failed!");
            Disconnect();   // in case it is not already disconnected
        }
    }
}
