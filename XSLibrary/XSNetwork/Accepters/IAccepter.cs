using System;
using System.Net.Sockets;
using XSLibrary.Utility;

namespace XSLibrary.Network.Accepters
{
    public delegate void ClientConnectedHandler(object sender, Socket acceptedSocket);

    public interface IAccepter : IDisposable
    {
        event ClientConnectedHandler ClientConnected;

        Logger Logger { get; set; }

        void Run();
    }
}
