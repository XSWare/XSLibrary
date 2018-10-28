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

    public abstract class ConnectionInterface
    {
        public delegate void DataReceivedHandler(object sender, byte[] data, IPEndPoint source);
        public event DataReceivedHandler DataReceivedEvent;

        public delegate void CommunicationErrorHandler(object sender, IPEndPoint remote);
        public event CommunicationErrorHandler OnSendError;
        public event CommunicationErrorHandler OnReceiveError;
        public event CommunicationErrorHandler OnDisconnect;     // can basically come from any thread so make your actions threadsafe

        public int MaxReceiveSize { get; set; } = 2048;     // MTU usually limits this to ~1450
        // timeout in milliseconds
        public int HandshakeTimeout { get; set; } = 5000;   

        public Logger Logger { get; set; }

        private SafeExecutor m_lock;

        public bool Connected { get { return m_lock.Execute(() => { return !Disconnecting && ConnectionSocket.Connected; }); } }

        volatile bool m_receiving = false;
        public bool Receiving
        {
            get { return m_receiving && !Disconnecting; }
            protected set
            {
                m_receiving = value;
            }
        }

        protected Socket ConnectionSocket { get; set; }
        public IPEndPoint Local { get; protected set; }
        public IPEndPoint Remote { get; protected set; }

        protected Thread ReceiveThread { get; set; }

        volatile bool m_sendEnabled;
        private volatile bool m_disconnected;
        private volatile bool m_preReceiveDone;

        protected bool Disconnecting
        {
            get { return m_disconnected; }
            set { m_disconnected = value; }
        }

        protected IConnectionCrypto Crypto { get; set; }

        public ConnectionInterface(Socket connectionSocket)
        {
            Crypto = new NoCrypto();

            Logger = new NoLog();
            m_lock = new SingleThreadExecutor();

            m_sendEnabled = true;
            m_preReceiveDone = false;

            InitializeSocket(connectionSocket);
        }

        public void Send(byte[] data)
        {
            SafeSend(() => SendSpecialized(Crypto.EncryptData(data)));
            Logger.Log("Sent data to {0}.", Remote.ToString());
        }

        protected void SafeSend(Action SendFunction)
        {
            bool error = false;
            m_lock.Execute(() =>
            {
                try
                {
                    if (m_sendEnabled && CanSend())
                        SendFunction();
                }
                catch (SocketException)
                {
                    m_sendEnabled = false;
                    error = true;
                }
                catch (ObjectDisposedException)
                {
                    m_sendEnabled = false;
                    error = true;
                }
                catch (CryptographicException)
                {
                    Logger.Log("Encryption error!");
                    m_sendEnabled = false;
                    error = true;
                }
            });

            // this should only occure once, even if multiple sends fail from different threads
            if(error)
                SendErrorHandling(Remote);
        }

        protected virtual bool CanSend()
        {
            return !Disconnecting;
        }

        protected abstract void SendSpecialized(byte[] data);

        protected void SendErrorHandling(IPEndPoint remote)
        {
            Disconnect();
            OnSendError?.Invoke(this, remote);
        }

        public bool InitializeCrypto(IConnectionCrypto crypto)
        {
            if (!m_lock.Execute(() => ExecuteCryptoHandshake(crypto)))
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
                if (Disconnecting)
                {
                    Logger.Log("Cannot intitiate crypto after disconnect!");
                    return false;
                }

                if (Receiving)
                {
                    Logger.Log("Crypto cannot be initiated after receive loop was started!");
                    return false;
                }

                ExecutePreReceiveActions();

                if (!crypto.Handshake(SendSpecialized, ReceiveSpecialized))
                    return false;

                Crypto = crypto;
                Logger.Log("Crypto handshake successful.");
                return true;
            }
            catch { return false; }
            finally { ConnectionSocket.ReceiveTimeout = previousTimeout; }
        }

        private void HandleHandshakeFailure()
        {
            Logger.Log("Crypto handshake failed!");
            Disconnect();
        }

        public void InitializeReceiving()
        {
            m_lock.Execute(UnsafeInitializeReceiving);
        }

        private void UnsafeInitializeReceiving()
        {
            if (Disconnecting)
            {
                Logger.Log("Can not start receiving from a disconnected connection!");
                return;
            }

            if (!Receiving)
            {
                Receiving = true;
                ExecutePreReceiveActions();
                StartReceiving();
            }
        }

        private void ExecutePreReceiveActions()
        {
            if (!m_preReceiveDone)
            {
                m_preReceiveDone = true;
                PreReceiveSettings();
            }
        }

        protected abstract void PreReceiveSettings();

        private void InitializeSocket(Socket socket)
        {
            m_lock.Execute(() => UnsafeInitializeSocket(socket));
        }

        private void UnsafeInitializeSocket(Socket socket)
        {
            Disconnecting = false;
            ConnectionSocket = socket;
        }

        private void StartReceiving()
        {
            if (ReceiveThread != null && ReceiveThread.ThreadState != ThreadState.Unstarted)
                return;

            ReceiveThread = new Thread(ReceiveLoop);
            ReceiveThread.Name = "Connection receive";
            ReceiveThread.Start();
        }

        private void ReceiveLoop()
        {
            while (!Disconnecting)
            {
                try
                {
                    if (ReceiveSpecialized(out byte[] data, out IPEndPoint source))
                        RaiseReceivedEvent(Crypto.DecryptData(data), source);
                    else
                    {
                        ReceiveThread = null;
                        Disconnect();
                    }
                }
                catch (SocketException)
                {
                    ReceiveThread = null;
                    ReceiveErrorHandling(Remote);
                }
                catch (ObjectDisposedException)
                {
                    ReceiveThread = null;
                    ReceiveErrorHandling(Remote);
                }
                catch (CryptographicException)
                {
                    Logger.Log("Decryption error!");
                    ReceiveThread = null;
                    ReceiveErrorHandling(Remote);
                }
            }

            Receiving = false;
        }

        protected abstract bool ReceiveSpecialized(out byte[] data, out IPEndPoint source);

        protected void ReceiveErrorHandling(IPEndPoint remote)
        {
            Disconnect();
            OnReceiveError?.Invoke(this, remote);
        }

        private void RaiseReceivedEvent(byte[] data, IPEndPoint source)
        {
            Logger.Log("Received data from {0}.", source.ToString());
            DataReceivedEvent?.Invoke(this, data, source);
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
            if(m_lock.Execute(UnsafeDisconnect))
            {
                WaitForDisconnect();
                Logger.Log("Disconnected.");
                RaiseOnDisconnect();
                DataReceivedEvent = null;
            }
        }

        private bool UnsafeDisconnect()
        {
            if (!Disconnecting)
            {
                Disconnecting = true;

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