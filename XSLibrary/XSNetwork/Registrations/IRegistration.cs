using System;
using System.Collections.Generic;
using System.Net.Sockets;
using XSLibrary.Cryptography.AccountManagement;
using XSLibrary.Cryptography.ConnectionCryptos;
using XSLibrary.Network.Accepters;
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
        public CryptoType Crypto = CryptoType.NoCrypto;

        private IAccepter Accepter { get; set; }
        protected IUserDataBase DataBase { get; private set; }
        protected IAccountPool<AccountType> Accounts { get; private set; }

        public IRegistration(TCPAccepter accepter, IUserDataBase dataBase, IAccountPool<AccountType> initialAccounts)
        {
            Accepter = accepter;
            DataBase = dataBase;
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

            if (!Authenticate(out AccountType user, connection))
            {
                Logger.Log(LogLevel.Error, "Authentication failed.");
                Accounts.DisposeAccount(user);
                connection.Disconnect();
                return;
            }

            HandleVerifiedConnection(user, connection);
            Accounts.AddAccount(user);
        }

        protected abstract ConnectionType CreateConnection(Socket acceptedSocket);

        protected abstract bool Authenticate(out AccountType user, ConnectionType connection);

        protected abstract void HandleVerifiedConnection(AccountType user, ConnectionType clientConnection);

        public virtual void Dispose()
        {
            Accepter.Dispose();
        }
    }
}
