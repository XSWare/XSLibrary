﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using XSLibrary.Network.Connections;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connectors
{
    public abstract class Connector<ConnectionType> where ConnectionType: TCPConnection
    {
        public static string MessageConnecting { get; set; } = "Connecting...";
        public static string MessageFailedConnect { get; set; } = "Failed to connect!";
        public static string MessageSuccess { get; set; } = "Successfully connected.";

        public Logger Logger { get; set; } = Logger.NoLog;

        public bool CurrentlyConnecting { get; set; } = false;
        public int TimeoutConnect { get; set; } = 5000;
        public bool LastConnectSuccessfull { get => LastConnect != null; }

        EndPoint LastConnect { get; set; } = null;

        const string RECONNECT_EMPTY = "Previous connect attempt failed or no attempt was made yet.";

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

            return ConnectInternal(remote, out connection);
        }

        public void ConnectAsync(EndPoint remote, Action<ConnectionType> SuccessCallback, Action ErrorCallback)
        {
            if (CurrentlyConnecting)
                return;

            CurrentlyConnecting = true;

            new Task(() =>
            {
                if (ConnectInternal(remote, out ConnectionType connection))
                    SuccessCallback(connection);
                else
                    ErrorCallback();
            }).Start();
        }

        public bool Reconnect(out ConnectionType connection, out string message)
        {
            connection = null;
            message = "";

            if (!LastConnectSuccessfull)
            {
                message = RECONNECT_EMPTY;
                return false;
            }

            return Connect(LastConnect, out connection, out message);
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
                Logger.Log(LogLevel.Information, MessageConnecting);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IAsyncResult result = socket.BeginConnect(remote, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(TimeoutConnect, true);

                if(!socket.Connected)
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