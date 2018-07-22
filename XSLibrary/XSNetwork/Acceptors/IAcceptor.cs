using System;
using System.Net.Sockets;
using XSLibrary.Utility;

namespace XSLibrary.Network.Acceptors
{
    public delegate void ClientConnectedHandler(object sender, Socket acceptedSocket);

    public interface IAcceptor : IDisposable
    {
        event ClientConnectedHandler ClientConnected;

        Logger Logger { get; set; }

        void Run();
    }
}
