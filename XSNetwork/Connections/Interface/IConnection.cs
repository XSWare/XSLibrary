using System;
using System.Net;
using System.Net.Sockets;
using XSLibrary.Cryptography.ConnectionCryptos;
using XSLibrary.ThreadSafety.Events;
using XSLibrary.ThreadSafety.Executors;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    using OnDisconnectEvent = OneShotEvent<object, EndPoint>;

    public class ConnectionException : Exception
    {
        public ConnectionException(string exceptionMessage) : this(exceptionMessage, null) { }
        public ConnectionException(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException) { }
    }

    public abstract partial class IConnection : IDisposable
    {
        /// <summary>
        /// Can come from any thread so make your actions threadsafe
        /// </summary>
        public IEvent<object, EndPoint> OnDisconnect { get { return DisconnectHandle; } }
        private OnDisconnectEvent DisconnectHandle = new OnDisconnectEvent();

        public virtual Logger Logger { get; set; }

        public bool Connected { get { return m_connectLock.ExecuteRead(() => { return !m_disconnecting && ConnectionSocket.Connected; }); } }

        volatile bool m_receiving = false;
        public bool Receiving
        {
            get { return m_receiving && !m_disconnecting; }
            protected set
            {
                m_receiving = value;
            }
        }

        protected Socket ConnectionSocket { get; set; }
        public EndPoint Local { get; protected set; }
        public EndPoint Remote { get; protected set; }

        private SafeReadWriteExecutor m_connectLock = new RWExecutorWinNative();
        volatile bool m_preReceiveDone;
        volatile bool m_disconnecting;

        protected IConnectionCrypto Crypto { get; set; }

        public IConnection(Socket connectionSocket)
        {
            Crypto = new NoCrypto();
            Logger = Logger.NoLog;

            m_preReceiveDone = false;

            InitializeSocket(connectionSocket);
        }

        private void InitializeSocket(Socket socket)
        {
            m_connectLock.Execute(() =>
            {
                m_disconnecting = false;
                ConnectionSocket = socket;
            });
        }

        protected void TrimData(ref byte[] data, int size)
        {
            if (data.Length <= size)
                return;

            Array.Resize(ref data, size);
        }

        public void Kill()
        {
            Disconnect(0);
        }

        // timeout is in milliseconds and signals how much time the peer has to finish their cleanup of the connection
        public void Disconnect(int timeout = 10000)
        {
            bool soft = timeout > 0;

            if (m_connectLock.Execute(() =>
             {
                 if (!m_disconnecting)
                 {
                     m_disconnecting = true;

                     ShutdownSocket();
                     if (!soft)
                         CloseSocket();

                     return true;
                 }

                 return false;
             }))
            {
                WaitReceiveThread(timeout);
                if(soft)
                    m_connectLock.Execute(CloseSocket);
                Logger.Log(LogLevel.Information, "Disconnected from {0}.", Remote.ToString());
                RaiseOnDisconnect();
            }
        }

        private void ShutdownSocket()
        {
            try
            {
                ConnectionSocket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException ex)
            {
                Logger.Log(LogLevel.Error, ex.Message);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                throw new ConnectionException("Exception while shutting down socket! Exception message: " + ex.Message, ex);
            }
        }

        private void CloseSocket()
        {
            try
            {
                ConnectionSocket.Close();
            }
            catch (SocketException ex)
            {
                Logger.Log(LogLevel.Error, ex.Message);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                throw new ConnectionException("Exception while closing socket! Exception message: " + ex.Message, ex);
            }
        }

        private void RaiseOnDisconnect()
        {
            DisconnectHandle.Invoke(this, Remote);
        }

        public void Dispose()
        {
            Kill();
        }
    }
}