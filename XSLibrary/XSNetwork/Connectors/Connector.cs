using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using XSLibrary.Network.Connections;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connectors
{
    public abstract class Connector<ConnectionType> where ConnectionType: TCPConnection
    {
        Logger Logger { get; set; } = Logger.NoLog;

        public bool CurrentlyConnecting { get; set; } = false;
        int TimeoutConnect { get; set; } = 5000;

        public Connector()
        {

        }

        public bool Connect(EndPoint remote, out ConnectionType connection, out string message)
        {
            connection = null;
            message = "";

            if (CurrentlyConnecting)
                return false;

            CurrentlyConnecting = true;

            return ConnectInternal(remote, out connection, out message);
        }

        public void ConnectAsync(EndPoint remote, Action<ConnectionType> SuccessActions, Action<string> ErrorActions)
        {
            if (CurrentlyConnecting)
                return;

            CurrentlyConnecting = true;

            new Task(() =>
            {
                if (ConnectInternal(remote, out ConnectionType connection, out string message))
                    SuccessActions(connection);
                else
                    ErrorActions(message);
            }).Start();
        }

        private bool ConnectInternal(EndPoint remote, out ConnectionType connection, out string message)
        {
            connection = null;
            message = "";

            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IAsyncResult result = socket.BeginConnect(remote, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(TimeoutConnect, true);

                if(!socket.Connected)
                {
                    socket.Close();
                    message = "Failed to connect.";
                    return false;
                }
                else
                    socket.EndConnect(result);

                try { connection = InitializeConnection(socket); }
                catch (Exception exception)
                {
                    message = exception.Message;
                    return false;
                }

                message = "Successfully connected.";
                return true;
            }
            finally { CurrentlyConnecting = false; }
        }

        protected abstract ConnectionType InitializeConnection(Socket connectedSocket);
    }
}
