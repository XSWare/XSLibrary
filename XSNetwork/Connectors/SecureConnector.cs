using System.Net.Sockets;
using XSLibrary.Cryptography.ConnectionCryptos;
using XSLibrary.Network.Connections;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connectors
{
    public class SecureConnector : Connector<TCPPacketConnection>
    {
        public static string MessageInitiatingHandshake { get; set; } = "Encrypting connection...";
        public static string MessageHandshakeFailed { get; set; } = "Handshake failed.";

        public CryptoType Crypto { get; set; } = CryptoType.NoCrypto;
        public int TimeoutCryptoHandshake { get; set; } = 5000;

        protected override TCPPacketConnection InitializeConnection(Socket connectedSocket)
        {
            TCPPacketConnection connection = new TCPPacketConnection(connectedSocket);

            Logger.Log(LogLevel.Information, MessageInitiatingHandshake);
            if (!connection.InitializeCrypto(CryptoFactory.CreateCrypto(Crypto, true), TimeoutCryptoHandshake))
                throw new ConnectException(MessageHandshakeFailed);

            return connection;
        }
    }
}
