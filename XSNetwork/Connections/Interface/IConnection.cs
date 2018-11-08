using System;
using System.Net;
using System.Net.Sockets;
using XSLibrary.Cryptography.ConnectionCryptos;
using XSLibrary.ThreadSafety.Events;
using XSLibrary.ThreadSafety.Executors;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    using OnDisconnectEvent = AutoInvokeEvent<object, EndPoint>;

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

        protected byte[] TrimData(byte[] data, int size)
        {
            if (data.Length <= size)
                return data;

            byte[] returnData = new byte[size];
            Array.Copy(data, 0, returnData, 0, size);
            return returnData;
        }

        public void Disconnect()
        {
            if(m_connectLock.Execute(CloseSocket))
            {
                WaitReceiveThread();
                Logger.Log(LogLevel.Information, "Disconnected from {0}.", Remote.ToString());
                RaiseOnDisconnect();
            }
        }

        private bool CloseSocket()
        {
            if (!m_disconnecting)
            {
                m_disconnecting = true;

                try
                {
                    ConnectionSocket.Shutdown(SocketShutdown.Both);
                    ConnectionSocket.Close();
                }
                catch (SocketException ex)
                {
                    Logger.Log(LogLevel.Error, ex.Message);
                }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    throw new ConnectionException("Exception while disconnecting! Exception message: " + ex.Message, ex);
                }

                return true;
            }

            return false;
        }

        private void RaiseOnDisconnect()
        {
            DisconnectHandle.Invoke(this, Remote);
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}