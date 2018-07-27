using System;
using System.Net;
using System.Net.Sockets;
using XSLibrary.Network.Connections;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connectors
{
    public class ConnectException : Exception
    {
        public ConnectException() { }
        public ConnectException(string message) : base(message) { }
    }

    public abstract class Connector<ConnectionType> where ConnectionType: TCPConnection
    {
        public static string MessageConnecting { get; set; } = "Connecting to {0}...";
        public static string MessageFailedConnect { get; set; } = "Failed to connect!";
        public static string MessageSuccess { get; set; } = "Successfully connected.";

        public Logger Logger { get; set; } = Logger.NoLog;

        public bool CurrentlyConnecting { get; set; } = false;
        public int TimeoutConnect { get; set; } = 5000;
        public bool LastConnectSuccessfull { get => LastConnect != null; }

        EndPoint LastConnect { get; set; } = null;

        const string RECONNECT_EMPTY = "Previous connect attempt failed or no attempt was made yet.";

        protected Connector()
        {
        }

        public bool Connect(EndPoint remote, out ConnectionType connection)
        {
            connection = null;

            if (CurrentlyConnecting)
                return false;

            CurrentlyConnecting = true;

            return ConnectInternal(remote, out connection);
        }

        public void ConnectAsync(EndPoint remote, Action<ConnectionType> SuccessCallback, Action ErrorCallback)
        {
            if (CurrentlyConnecting)
                return;

            CurrentlyConnecting = true;

            ThreadStarter.ThreadpoolDebug("Connector", () =>
            {
                if (ConnectInternal(remote, out ConnectionType connection))
                    SuccessCallback(connection);
                else
                    ErrorCallback();
            });
        }

        public bool Reconnect(out ConnectionType connection)
        {
            connection = null;

            if (!LastConnectSuccessfull)
            {
                Logger.Log(LogLevel.Error, RECONNECT_EMPTY);
                return false;
            }

            return Connect(LastConnect, out connection);
        }

        public void ReconnectAsync(Action<ConnectionType> SuccessCallback, Action ErrorCallback)
        {
            if (!LastConnectSuccessfull)
            {
                ErrorCallback();
                return;
            }

            ConnectAsync(LastConnect, SuccessCallback, ErrorCallback);
        }

        private bool ConnectInternal(EndPoint remote, out ConnectionType connection)
        {
            connection = null;
            LastConnect = null;

            try
            {
                Logger.Log(LogLevel.Information, MessageConnecting, remote);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IAsyncResult result = socket.BeginConnect(remote, null, null);
                result.AsyncWaitHandle.WaitOne(TimeoutConnect, true);

                if (!socket.Connected)
                {
                    socket.Close();
                    Logger.Log(LogLevel.Error, MessageFailedConnect);
                    return false;
                }
                else
                    socket.EndConnect(result);

                try { connection = InitializeConnection(socket); }
                catch (Exception exception)
                {
                    Logger.Log(LogLevel.Error, exception.Message);
                    return false;
                }

                Logger.Log(LogLevel.Information, MessageSuccess);
                LastConnect = remote;
                return true;
            }
            finally { CurrentlyConnecting = false; }
        }

        protected abstract ConnectionType InitializeConnection(Socket connectedSocket);
    }
}
