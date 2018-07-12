using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace XSLibrary.Network.Connections
{
    public abstract partial class IConnection
    {
        public void Send(byte[] data)
        {
            if (SafeSend(() => SendSpecialized(Crypto.EncryptData(data))))
                Logger.Log("Sent data to {0}.", Remote.ToString());
        }

        protected bool SafeSend(Action SendFunction)
        {
            return m_sendLock.Execute(() =>
            {
                bool error = false;
                try
                {
                    if (!m_connectLock.ExecuteReadonly(() =>
                    {
                        if (CanSend())
                        {
                            SendFunction();
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
                    Logger.Log("Encryption error!");
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
            OnSendError?.Invoke(this, remote);
        }
    }
}
