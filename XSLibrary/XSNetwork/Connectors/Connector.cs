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

        public void Connect(EndPoint remote, Action<ConnectionType> SuccessActions, Action<string> ErrorActions)
        {
            if (CurrentlyConnecting)
                return;

            CurrentlyConnecting = true;

            new Task(() => ConnectAsync(remote, SuccessActions, ErrorActions)).Start();
        }

        private void ConnectAsync(EndPoint remote, Action<ConnectionType> SuccessActions, Action<string> ErrorActions)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IAsyncResult result = socket.BeginConnect(remote, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(TimeoutConnect, true);

                if(!socket.Connected)
                {
                    socket.Close();
                    ErrorActions("Failed to connect.");
                    return;
                }
                else
                    socket.EndConnect(result);

                try { SuccessActions(InitializeConnection(socket)); }
                catch (Exception exception)
                {
                    ErrorActions(exception.Message);
                }
            }
            finally { CurrentlyConnecting = false; }
        }

        protected abstract ConnectionType InitializeConnection(Socket connectedSocket);
    }
}
