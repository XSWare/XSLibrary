using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using XSLibrary.ThreadSafety.Executors;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public abstract partial class IConnection
    {
        public int SendTimeout
        {
            get { return ConnectionSocket.SendTimeout; }
            set { ConnectionSocket.SendTimeout = value; }
        }

        private SafeExecutor m_sendLock = new SingleThreadExecutor();

        public bool Send(byte[] data, int timeout = -1)
        {
            if (SafeSend(() => SendSpecialized(Crypto.EncryptData(data)), timeout))
            {
                Logger.Log(LogLevel.Information, "Sent data to {0}.", Remote.ToString());
                return true;
            }
            else
                return false;
        }

        protected bool SafeSend(Action SendFunction, int timeout = -1)
        {
            return m_sendLock.Execute(() =>
            {
                bool error = false;
                try
                {
                    int sendTimeout = ConnectionSocket.SendTimeout;
                    if (timeout > -1)
                        ConnectionSocket.SendTimeout = timeout;

                    if (!m_connectLock.ExecuteReadonly(() =>
                    {
                        if (CanSend())
                        {
                            SendFunction();
                            if (timeout > -1)
                                ConnectionSocket.SendTimeout = sendTimeout;
                            return true;
                        }
                        else
                            return false;
                    }))
                        return false;
                }
                catch (SocketException)
                {
                    error = true;
                }
                catch (ObjectDisposedException)
                {
                    error = true;
                }
                catch (CryptographicException)
                {
                    Logger.Log(LogLevel.Error, "Encryption error!");
                    error = true;
                }

                // this should only occure once, even if multiple sends fail from different threads
                if (error)
                    SendErrorHandling(Remote);

                return !error;
            });
        }

        protected virtual bool CanSend()
        {
            return !m_disconnecting;
        }

        protected abstract void SendSpecialized(byte[] data);

        protected void SendErrorHandling(EndPoint remote)
        {
            Disconnect();
        }
    }
}
