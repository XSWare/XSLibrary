using System.Text;
using XSLibrary.Network.Connections;

namespace XSLibrary.Utility
{
    public class NetworkLogger : Logger
    {
        public IConnection Connection { get; set; }

        protected override void LogMessage(string text)
        {
            if (Connection != null)
                Connection.Send(Encoding.ASCII.GetBytes(text));
        }
    }
}
