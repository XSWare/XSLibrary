using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using XSLibrary.ThreadSafety.Executors;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public abstract partial class IConnection
    {
        public delegate void DataReceivedHandler(object sender, byte[] data, EndPoint source);
        public event DataReceivedHandler DataReceivedEvent;

        public int ReceiveBufferSize { get; set; } = 2048;  // increase to have better performance for big data chunks with the cost of using more RAM
        public int ReceiveTimeout
        {
            get { return ConnectionSocket.ReceiveTimeout; }
            set { ConnectionSocket.ReceiveTimeout = value; }
        }

        protected Thread ReceiveThread { get; set; }

        private SafeExecutor m_receiveLock = new SingleThreadExecutor();

        public void InitializeReceiving()
        {
            m_connectLock.Execute(UnsafeInitializeReceiving);
        }

        private void UnsafeInitializeReceiving()
        {
            if (m_disconnecting)
            {
                Logger.Log(Utility.LogLevel.Error, "Can not start receiving from a disconnected connection!");
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

        public bool Receive(out byte[] data, out EndPoint source) { return Receive(out data, out source, -1); }
        public bool Receive(out byte[] data, out EndPoint source, int timeout)
        {
            if (SafeReceive(out data, out source, timeout))
            {
                data = Crypto.DecryptData(data);
                return true;
            }
            else
                return false;
        }

        private bool SafeReceive(out byte[] data, out EndPoint source, int timeout = -1)
        {
            data = null;
            source = null;

            m_receiveLock.Lock();

            try
            {
                int receiveTimeout = ConnectionSocket.ReceiveTimeout;
                if (timeout > -1)
                    ConnectionSocket.ReceiveTimeout = timeout;

                if (ReceiveSpecialized(out data, out source))
                {
                    if (timeout > -1)
                        ConnectionSocket.ReceiveTimeout = receiveTimeout;
                    return true;
                }
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
                Logger.Log(LogLevel.Error, "Decryption error!");
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
        }

        private void RaiseReceivedEvent(byte[] data, EndPoint source)
        {
            Logger.Log(LogLevel.Information, "Received data from {0}.", source.ToString());
            DataReceivedEvent?.Invoke(this, data, source);
        }

        protected virtual void WaitReceiveThread()
        {
            ReceiveThread?.Join();
        }
    }
}
