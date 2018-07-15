using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using XSLibrary.Utility;

namespace XSLibrary.Network.Accepters
{
    public class TCPAccepter : IDisposable
    {
        public delegate void ClientConnectedHandler(object sender, Socket acceptedSocket);
        public event ClientConnectedHandler ClientConnected;

        public Logger Logger { get; set; }

        public int Port { get; private set; }
        public int MaxPendingConnections { get; private set; }

        private bool m_running;
        public bool Running { get { return m_running && !Abort; } }
        protected bool Abort { get; private set; }

        Socket m_listeningSocket;
        Thread m_acceptThread;

        public TCPAccepter(int port, int maxPendingConnections)
        { 
            Port = port;
            MaxPendingConnections = maxPendingConnections;

            m_running = false;
            Abort = false;

            Logger = new NoLog();

            m_listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Run()
        {
            if (m_running)
                return;

            m_running = true;

            m_listeningSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            m_listeningSocket.Listen(MaxPendingConnections);

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
            Logger.Log("Accepted connection from {0}", acceptedSocket.RemoteEndPoint.ToString());
            m_acceptThread = new Thread(() => RaiseClientConnectedEvent(acceptedSocket));
            m_acceptThread.Name = "Socket init routine";
            m_acceptThread.Start();
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
            Abort = true;

            try
            {
                m_listeningSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                m_listeningSocket.Dispose();
            }

            m_acceptThread.Join();
        }
    }
}
