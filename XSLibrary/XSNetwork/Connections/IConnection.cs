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

    public abstract class IConnection
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

        private SafeExecutor m_connectLock;
        private SafeExecutor m_sendLock;
        private SafeExecutor m_receiveLock;

        public bool Connected { get { return m_connectLock.Execute(() => { return !m_disconnecting && ConnectionSocket.Connected; }); } }

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
            m_connectLock = new SingleThreadExecutor();
            m_sendLock = new SingleThreadExecutor();
            m_receiveLock = new SingleThreadExecutor();

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
                    if (!m_connectLock.Execute(() =>
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

        public bool InitializeCrypto(IConnectionCrypto crypto)
        {
            if (!m_connectLock.Execute(() => ExecuteCryptoHandshake(crypto)))
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
                if (m_disconnecting)
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
            m_connectLock.Execute(UnsafeInitializeReceiving);
        }

        private void UnsafeInitializeReceiving()
        {
            if (m_disconnecting)
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
            while (!m_disconnecting)
            {
                if (SafeReceive(out byte[] data, out EndPoint source))
                    RaiseReceivedEvent(Crypto.DecryptData(data), source);
            }

            Receiving = false;
        }

        public bool Receive(out byte[] data, out EndPoint source)
        {
            if (SafeReceive(out data, out source))
            {
                data = Crypto.DecryptData(data);
                return true;
            }
            else
                return false;
        }

        private bool SafeReceive(out byte[] data, out EndPoint source)
        {
            data = null;
            source = null;

            m_receiveLock.Lock();
            try
            {
                if (ReceiveSpecialized(out data, out source))
                {
                    return true;
                }
                else
                {
                    Disconnect();
                    ReceiveThread = null;
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
            finally { m_receiveLock.Release(); }

            return false;
        }

        protected abstract bool ReceiveSpecialized(out byte[] data, out EndPoint source);

        protected void ReceiveErrorHandling(EndPoint remote)
        {
            Disconnect();
            OnReceiveError?.Invoke(this, remote);
        }

        private void RaiseReceivedEvent(byte[] data, EndPoint source)
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