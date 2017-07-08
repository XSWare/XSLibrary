using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using XSLibrary.Utility;

namespace XSLibrary.Network.Accepters
{
    public class Accepter : IDisposable
    {
        public delegate void ClientConnectedHandler(object sender, Socket acceptedSocket);
        public event ClientConnectedHandler ClientConnected;

        public Logger Logger { get; set; }

        public int Port { get; private set; }
        public bool Running { get; private set; }
        bool m_abort;

        Socket m_listeningSocket;
        Thread m_acceptThread;

        public Accepter(Socket listerner, int port, int maxNumberClients)
        {
            Port = port;
            Running = false;
            m_abort = false;

            Logger = new NoLog();

            m_listeningSocket = listerner;
            m_listeningSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            m_listeningSocket.Listen(maxNumberClients);
        }

        public void Run()
        {
            if (Running)
                return;

            Running = true;

            m_acceptThread = new Thread(AcceptLoop);
            m_acceptThread.Start();
        }

        private void AcceptLoop()
        {
            while (!m_abort)
            {
                Socket acceptedSocket;

                try
                {
                    acceptedSocket = m_listeningSocket.Accept();
                }
                catch { continue; }

                Logger.Log("Accepted connection from {0}", acceptedSocket.RemoteEndPoint.ToString());
                RaiseClientConnectedEvent(acceptedSocket);
            }
        }

        private void RaiseClientConnectedEvent(Socket acceptedSocket)
        {
            ClientConnectedHandler safeCopy = ClientConnected;
            if (safeCopy != null)
                safeCopy.Invoke(this, acceptedSocket);
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            m_abort = true;

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
