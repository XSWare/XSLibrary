using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using XSLibrary.ThreadSafety.Executors;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string exceptionMessage) : this (exceptionMessage, null)
        {
        }

        public ConnectionException(string exceptionMessage, Exception innerException) : base(exceptionMessage, innerException)
        {
        }
    }

    public abstract class ConnectionInterface
    {
        public event ReceiveErrorHandler ReceiveErrorEvent;
        public event EventHandler OnDisconnect;     // can basically come from any thread so make your actions threadsafe

        public delegate void ReceiveErrorHandler(object sender, IPEndPoint endPoint);

        public int MaxPacketSize { get; set; } = 8192;

        public Logger Logger { get; set; }

        protected SafeExecutor m_lock;

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
        protected virtual IPEndPoint Local { get { return ConnectionSocket.LocalEndPoint as IPEndPoint; } }
        protected virtual IPEndPoint Remote { get { return ConnectionSocket.RemoteEndPoint as IPEndPoint; } }

        protected Thread ReceiveThread { get; set; }

        private volatile bool m_disconnected;
        protected bool Disconnecting
        {
            get { return m_disconnected; }
            set { m_disconnected = value; }
        }

        public ConnectionInterface(Socket connectionSocket)
        {
            Logger = new NoLog();
            m_lock = new SingleThreadExecutor();

            InitializeSocket(connectionSocket);
        }

        public void Send(byte[] data)
        {
            m_lock.Execute(() => UnsafeSend(data));
        }

        protected abstract void UnsafeSend(byte[] data);

        public void InitializeReceiving()
        {
            m_lock.Execute(UnsafeInitializeReceiving);
        }

        private void UnsafeInitializeReceiving()
        {
            if (Disconnecting)
            {
                throw new ConnectionException("Can not receive from a disconnected connection!");
            }

            if (!Receiving)
            {
                Receiving = true;
                PreReceiveSettings();
                StartReceiving();
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
                    ReceiveFromSocket();
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
            }

            Receiving = false;
        }

        protected abstract void ReceiveFromSocket();

        protected void ReceiveErrorHandling(IPEndPoint source)
        {
            Disconnect();
            ReceiveErrorEvent?.Invoke(this, source);
        }

        protected byte[] TrimData(byte[] data, int size)
        {
            if (data.Length <= size)
                return data;

            byte[] returnData = new byte[size];
            Array.Copy(data, 0, returnData, 0, size);
            return returnData;
        }
        
        public bool ConnectionPolling()
        {
            if (Disconnecting)
                return false;

            bool connected = false;
            if (ConnectionSocket.Poll(0, SelectMode.SelectRead))
            {
                byte[] data = new byte[1];
                connected = ConnectionSocket.Receive(data, SocketFlags.Peek) != 0;
            }

            return connected;
        }

        public void Disconnect()
        {
            if(m_lock.Execute(UnsafeDisconnect))
            {
                WaitForDisconnect();
                Logger.Log("Disconnected.");
                RaiseOnDisconnect();
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
            OnDisconnect?.Invoke(this, new EventArgs());
        }
    }
}