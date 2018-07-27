using System;
using System.Net.Sockets;
using XSLibrary.Cryptography.ConnectionCryptos;
using XSLibrary.Network.Acceptors;
using XSLibrary.Network.Connections;
using XSLibrary.Utility;

namespace XSLibrary.Network.Registrations
{
    public abstract class IRegistration<ConnectionType, AccountType> : IDisposable 
        where ConnectionType : IConnection 
        where AccountType : IUserAccount
    {
        private Logger m_logger = Logger.NoLog;
        public Logger Logger
        {
            get { return m_logger; }
            set
            {
                m_logger = value;
                Accepter.Logger = m_logger;
                Accounts.Logger = m_logger;
            }
        }

        public int AuthenticationTimeout { get; set; } = 5000;
        public int CryptoHandshakeTimeout { get; set; } = 5000;
        public CryptoType Crypto { get; set; } = CryptoType.NoCrypto;

        private IAcceptor Accepter { get; set; }
        protected IAccountPool<AccountType> Accounts { get; private set; }

        public IRegistration(TCPAcceptor accepter, IAccountPool<AccountType> initialAccounts)
        {
            Accepter = accepter;
            Accounts = initialAccounts;
        }

        public void Run()
        {
            Accepter.ClientConnected += OnClientConnected;
            Accepter.Run();
        }

        void OnClientConnected(object sender, Socket acceptedSocket)
        {
            ConnectionType connection = CreateConnection(acceptedSocket);
            connection.Logger = Logger;

            if (!connection.InitializeCrypto(CryptoFactory.CreateCrypto(Crypto, false), CryptoHandshakeTimeout))
                return;

            if (!Authenticate(out string username, connection))
            {
                Logger.Log(LogLevel.Error, "Authentication failed from {0}", connection.Remote);
                connection.Disconnect();
                return;
            }

            HandleVerifiedConnection(Accounts.GetElement(username), connection);
            connection.OnDisconnect += (eventSender, arguments) => Accounts.ReleaseElement(username);
        }

        protected abstract ConnectionType CreateConnection(Socket acceptedSocket);

        protected abstract bool Authenticate(out string username, ConnectionType connection);

        protected abstract void HandleVerifiedConnection(AccountType user, ConnectionType clientConnection);

        public virtual void Dispose()
        {
            Accepter.Dispose();
            Logger.Log(LogLevel.Detail, "Registration disposed.");
        }
    }
}
