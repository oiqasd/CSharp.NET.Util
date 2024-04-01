using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public class SocketClient : IDisposable
    {
        TcpClient client = null;
        public SocketClient() { }
        public SocketClient(string host, int port = 80)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
            var ipAddress = ipHostInfo.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            client = new TcpClient();
            client.Client.Connect(ipAddress, port);
        }

        public TcpClient ConnectIP(string ip, int port)
        {
            if (client != null)
                throw new Exception("Client is created");
            client = new TcpClient();
            client.Client.Connect(IPAddress.Parse(ip), port);

            return client;
        }


        public void Send(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            client.Client.Send(messageBytes, SocketFlags.None);
        }

        public string Receive()
        {
            var buffer = new byte[1_024];
            var received = client.Client.Receive(buffer, SocketFlags.None);
            var message = Encoding.UTF8.GetString(buffer, 0, received);
            return message;
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Client.Shutdown(SocketShutdown.Both);
                client.Client.Disconnect(true);
                client.Close();
                client.Dispose();
            }
        }
    }
}
