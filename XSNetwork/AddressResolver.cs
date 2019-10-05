using System;
using System.Net;

namespace XSLibrary.Network
{
    public class AddressResolver
    {
        // resolve a string into a hostname/ip and port. returns null in case of an error.
        public static IPEndPoint Resolve(string addressString, int defaultPort = -1)
        {
            int port = defaultPort;
            string hostString = addressString;

            int portStart = addressString.LastIndexOf(':');
            if (portStart >= 0)
            {
                try
                {
                    port = Convert.ToInt32(addressString.Substring(portStart + 1));
                    hostString = addressString.Substring(0, portStart);
                }
                catch { port = defaultPort; }
            }

            if (port < 0)
                throw new Exception("Invalid port.");

            IPAddress ip;
            if (!IPAddress.TryParse(hostString, out ip))
            {
                // can only be hostname or garbage then
                IPHostEntry hostEntry = Dns.GetHostEntry(hostString);

                if (hostEntry.AddressList.Length > 0)
                    ip = hostEntry.AddressList[0];
                else
                    throw new Exception("Invalid hostname/IP.");
            }

            return new IPEndPoint(ip, port);
        }
    }
}
