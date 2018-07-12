using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using XSLibrary.Cryptography.ConnectionCryptos;
using XSLibrary.ThreadSafety.Executors;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string exceptionMessage) : this(exceptionMessage, null) { }
        public ConnectionException(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException) { }
    }

    public abstract partial class IConnection
    {
        public delegate void DataReceivedHandler(object sender, byte[] data, EndPoint source);
        public event DataReceivedHandler DataReceivedEvent;

        public delegate void CommunicationErrorHandler(object sender, EndPoint remote);
        public event CommunicationErrorHandler OnSendError;
        public event CommunicationErrorHandler OnReceiveError;
        public event CommunicationErrorHandler OnDisconnect;     // can basically come from any thread so make your actions threadsafe

        public int MaxReceiveSize { get; set; } = 2048;     // MTU usually limits this to ~1450
        // timeout in milliseconds
        public int HandshakeTimeout { get; set; } = 5000;
        public int SendTimeout => ConnectionSocket.SendTimeout;
        public int ReceiveTimeout => ConnectionSocket.ReceiveTimeout;

        public Logger Logger { get; set; }

        private SafeReadWriteExecutor m_connectLock;
        private SafeExecutor m_sendLock;
        private SafeExecutor m_receiveLock;
        private SafeExecutor m_handshakeLock;

        public bool Connected { get { return m_connectLock.ExecuteReadonly(() => { return !m_disconnecting && ConnectionSocket.Connected; }); } }

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

        protected Thread ReceiveThread { get; set; }

        volatile bool m_preReceiveDone;
        volatile bool m_disconnecting;

        protected IConnectionCrypto Crypto { get; set; }

        public IConnection(Socket connectionSocket)
        {
            Crypto = new NoCrypto();

            Logger = new NoLog();
            m_connectLock = new RWExecutorWinNative();
            m_sendLock = new SingleThreadExecutor();
            m_receiveLock = new SingleThreadExecutor();
            m_handshakeLock = new SingleThreadExecutor();

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
                WaitForDisconnect();
                Logger.Log("Disconnected.");
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
                    Logger.Log(ex.Message);
                }
                catch (Exception ex)
                {
                    throw new ConnectionException("Exception while disconnecting! Exception message: " + ex.Message, ex);
                }

                return true;
            }

            return false;
        }

        protected virtual void WaitForDisconnect()
        {
            ReceiveThread?.Join();
        }

        private void RaiseOnDisconnect()
        {
            OnDisconnect?.Invoke(this, Remote);
        }
    }
}