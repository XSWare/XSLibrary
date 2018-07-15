using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using XSLibrary.ThreadSafety.Executors;
using System.Collections.Generic;

namespace XSLibrary.Network.Accepters
{
    public class GuardedAccepter : TCPAccepter
    {
        Dictionary<string, int> Filter { get; set; } = new Dictionary<string, int>();
        SafeExecutor m_lock;

        public int ReduceInterval { get; set; } = 10000;
        public int BlockCount { get; set; } = 3;

        public GuardedAccepter(int port, int maxPendingConnection) : base(port, maxPendingConnection)
        {
            m_lock = new SingleThreadExecutor();

            Thread reduce = new Thread(ReduceLoop);
            reduce.Name = "Reduce loop";
            reduce.Start();
        }

        protected override void HandleAcceptedSocket(Socket acceptedSocket)
        {
            bool accept = false;

            m_lock.Execute(() =>
            {
                string key = (acceptedSocket.RemoteEndPoint as IPEndPoint).Address.ToString();

                if (!Filter.ContainsKey(key))
                    Filter.Add(key, 0);

                Filter[key]++;

                accept = Filter[key] < BlockCount;
            });

            if (accept)
                base.HandleAcceptedSocket(acceptedSocket);
            else
            {
                Logger.Log("Rejected connection from " + acceptedSocket.RemoteEndPoint.ToString());
                acceptedSocket.Dispose();
            }
        }

        private void ReduceLoop()
        {
            while(!Abort)
            {
                m_lock.Execute(() =>
                {
                    List<string> keys = new List<string>(Filter.Keys);
                    foreach (string key in keys)
                        Filter[key] = Math.Max(Filter[key] - 1, 0);
                });

                Thread.Sleep(ReduceInterval);
            }
        }
    }
}
