using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using XSLibrary.Cryptography.ConnectionCryptos;
using XSLibrary.Network.Connections;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connectors
{
    public class AccountConnector : Connector<TCPPacketConnection>
    {
        public static string MessageInitiatingHandshake { get; set; } = "Encrypting connection...";
        public static string MessageHandshakeFailed { get; set; } = "Handshake failed.";
        public static string MessageInitiatingAuthentication { get; set; } = "Authenticating...";
        public static string MessageAuthenticationFailed = "Authentication failed.";

        public CryptoType Crypto { get; set; } = CryptoType.NoCrypto;
        public int TimeoutCryptoHandshake { get; set; } = 5000;
        public int TimeoutAuthentication { get; set; } = 5000;
        public string Login { get; set; } = "";
        public string SuccessResponse { get; set; } = "+";

        protected override TCPPacketConnection InitializeConnection(Socket connectedSocket)
        {
            TCPPacketConnection connection = new TCPPacketConnection(connectedSocket);

            Logger.Log(LogLevel.Information, MessageInitiatingHandshake);
            if (!connection.InitializeCrypto(CryptoFactory.CreateCrypto(Crypto, true), TimeoutCryptoHandshake))
                throw new Exception(MessageHandshakeFailed);

            Logger.Log(LogLevel.Information, MessageInitiatingAuthentication);
            if (!connection.Send(Encoding.ASCII.GetBytes(Login), TimeoutAuthentication))
                HandleAuthenticationFailure(connection);

            if (!connection.Receive(out byte[] data, out EndPoint source, TimeoutAuthentication) || Encoding.ASCII.GetString(data) != SuccessResponse)
                HandleAuthenticationFailure(connection);

            return connection;
        }

        private void HandleAuthenticationFailure(TCPPacketConnection connection)
        {
            Login = "";
            throw new Exception(MessageAuthenticationFailed);
        }
    }
}
