using System.Net;
using System.Net.Sockets;
using System.Threading;
using XSLibrary.Utility;

namespace XSLibrary.Network.Acceptors
{
    public class TCPAcceptor : IAcceptor
    {
        public event ClientConnectedHandler ClientConnected;
        public Logger Logger { get; set; }

        public int Port { get; private set; }
        public int MaxPendingConnections { get; private set; }

        private bool m_running;
        public bool Running { get { return m_running && !Abort; } }

        volatile bool m_abort;
        protected bool Abort
        {
            get { return m_abort; }
            private set { m_abort = value; }
        }

        Socket m_listeningSocket;
        Thread m_acceptThread;

        public TCPAcceptor(int port, int maxPendingConnections)
        { 
            Port = port;
            MaxPendingConnections = maxPendingConnections;

            m_running = false;
            Abort = false;

            Logger = Logger.NoLog;

            m_listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Run()
        {
            if (m_running)
                return;

            m_running = true;

            m_listeningSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            m_listeningSocket.Listen(MaxPendingConnections);

            Logger.Log(LogLevel.Warning, "Now accepting incoming connections on port {0}.", Port);

            StartParallelLoops();
        }

        protected virtual void StartParallelLoops()
        {
            m_acceptThread = new Thread(AcceptLoop);
            m_acceptThread.Name = "Socket accept";
            m_acceptThread.Start();
        }

        private void AcceptLoop()
        {
            while (!Abort)
            {
                Socket acceptedSocket;

                try
                {
                    acceptedSocket = m_listeningSocket.Accept();
                }
                catch { continue; }

                HandleAcceptedSocket(acceptedSocket);
            }
        }

        protected virtual void HandleAcceptedSocket(Socket acceptedSocket)
        {
            Logger.Log(LogLevel.Information, "Accepted connection from {0} on port {1}", acceptedSocket.RemoteEndPoint, Port);
            DebugTools.ThreadpoolStarter("Socket init routine", () => RaiseClientConnectedEvent(acceptedSocket));
        }

        private void RaiseClientConnectedEvent(Socket acceptedSocket)
        {
            ClientConnected?.Invoke(this, acceptedSocket);
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            Logger.Log(LogLevel.Detail, "Disposing acceptor listening on port {0}", Port);

            Abort = true;

            try
            {
                m_listeningSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                m_listeningSocket.Dispose();
            }

            if(m_acceptThread != null && m_acceptThread.ThreadState != ThreadState.Unstarted)
                m_acceptThread.Join();

            Logger.Log(LogLevel.Detail, "Acceptor listening on port {0} disposed.", Port);
        }
    }
}
