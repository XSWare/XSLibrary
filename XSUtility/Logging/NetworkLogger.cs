using System.Text;
using XSLibrary.MultithreadingPatterns.Actor;
using XSLibrary.Network.Connections;

namespace XSLibrary.Utility
{
    /// <summary>
    /// Logging the same connection which is used for logging results (obviously) in an endless loop
    /// </summary>
    public class NetworkLogger : Logger
    {
        class LogActor : Actor<string>
        {
            public IConnection Connection { get; set; }

            public LogActor() : base("Network log")
            {

            }

            protected override void HandleMessage(string message)
            {
                if (Connection != null)
                    Connection.Send(Encoding.ASCII.GetBytes(message));
            }
        }

        public IConnection Connection
        {
            get { return actor.Connection; }
            set { actor.Connection = value; }
        }

        LogActor actor = new LogActor();

        protected override void LogMessage(string text)
        {
            // use actor to send messages to avoid any kind of stalling
            actor.SendMessage(text);
        }

        public override void Dispose()
        {
            base.Dispose();
            actor.Stop(false);
        }
    }
}
