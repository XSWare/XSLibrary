using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using XSLibrary.Cryptography.ConnectionCryptos;
using XSLibrary.Network.Connections;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connectors
{
    public class AccountConnector : SecureConnector
    {
        public static string MessageInitiatingAuthentication { get; set; } = "Authenticating...";
        public static string MessageAuthenticationFailed = "Authentication failed.";

        public int TimeoutAuthentication { get; set; } = 5000;
        public string Login { get; set; } = "";
        public string SuccessResponse { get; set; } = "+";

        protected override TCPPacketConnection InitializeConnection(Socket connectedSocket)
        {
            TCPPacketConnection connection = base.InitializeConnection(connectedSocket);

            if (!connection.Send(Encoding.ASCII.GetBytes(Login), TimeoutAuthentication))
                HandleAuthenticationFailure();

            Logger.Log(LogLevel.Information, MessageInitiatingAuthentication);

            if (!connection.Receive(out byte[] data, out EndPoint source, TimeoutAuthentication) || Encoding.ASCII.GetString(data) != SuccessResponse)
                HandleAuthenticationFailure();

            return connection;
        }

        private void HandleAuthenticationFailure()
        {
            Login = "";
            throw new ConnectException(MessageAuthenticationFailed);
        }
    }
}
