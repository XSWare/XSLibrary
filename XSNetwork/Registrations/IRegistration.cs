using System;
using System.Net;
using XSLibrary.Cryptography.ConnectionCryptos;
using XSLibrary.Network.Acceptors;
using XSLibrary.Network.Connections;
using XSLibrary.ThreadSafety.Events;
using XSLibrary.Utility;

namespace XSLibrary.Network.Registrations
{
    public abstract class IRegistration<AccountType> : IDisposable 
        where AccountType : IUserAccount
    {
        OneShotEvent<object, object> DisposeCalled = new OneShotEvent<object, object>();

        private Logger m_logger = Logger.NoLog;
        public Logger Logger
        {
            get { return m_logger; }
            set
            {
                m_logger = value;
                Acceptor.Logger = m_logger;
                Accounts.Logger = m_logger;
            }
        }

        public int AuthenticationTimeout { get; set; } = 5000;
        public int CryptoHandshakeTimeout { get; set; } = 5000;
        public CryptoType Crypto 
        { 
            get { return Acceptor.Crypto; }
            set { Acceptor.Crypto = value; }
        }

        private SecureAcceptor Acceptor { get; set; }
        protected IAccountPool<AccountType> Accounts { get; private set; }

        protected IRegistration(SecureAcceptor acceptor, IAccountPool<AccountType> initialAccounts)
        {
            Acceptor = acceptor;
            Accounts = initialAccounts;
        }

        public void Run()
        {
            Acceptor.SecureConnectionEstablished += OnClientConnected;
            Acceptor.Run();
        }

        void OnClientConnected(object sender, TCPPacketConnection connection)
        { 
            connection.Logger = Logger;

            if (!Authenticate(out string username, connection))
            {
                Logger.Log(LogLevel.Error, "Authentication failed from {0}", connection.Remote);
                connection.Kill();
                return;
            }

            HandleVerifiedConnection(Accounts.GetElement(username), connection);

            connection.OnDisconnect.Event += (eventSender, arguments) => Accounts.ReleaseElement(username);
            OneShotEvent<object, object>.EventHandle disposeHandler = (object disposeSender, object disposer) => { connection.Dispose(); };
            DisposeCalled.Event += disposeHandler;
            connection.OnDisconnect.Event += (object disconnectSender, EndPoint endpoint) => { DisposeCalled.Event -= disposeHandler; };
        }

        protected abstract bool Authenticate(out string username, TCPPacketConnection connection);

        protected abstract void HandleVerifiedConnection(AccountType user, TCPPacketConnection clientConnection);

        public virtual void Dispose()
        {
            Acceptor.Dispose();
            DisposeCalled?.Invoke(this, this);
            Logger.Log(LogLevel.Detail, "Registration disposed.");
        }
    }
}
