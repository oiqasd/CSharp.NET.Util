using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public class SocketHandler : IDisposable
    {
        Socket client = null;
        public SocketHandler(Socket socket)
        {
            client = socket;
            client.SendTimeout = 2000;
            client.ReceiveTimeout = 2000;
            
            //client.ReceiveBufferSize = 1024;
        }
        private IPEndPoint EndPoint { get { return client.RemoteEndPoint as IPEndPoint; } }
        public string Address { get { return EndPoint.Address.ToString(); } }
        public int Port { get { return EndPoint.Port; } }

        public async Task<string> Receive(string encoding = "utf-8")
        {
            var buffer = new byte[1_024];
            string data = ""; int received = 0;
#if NET
            do
            {
                received = await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                data += Encoding.GetEncoding(encoding).GetString(buffer, 0, received);
            } while (received == buffer.Length);
#else
            received = client.Receive(buffer, SocketFlags.None);
            data = Encoding.GetEncoding(encoding).GetString(buffer, 0, received);
#endif
            return data;
        }

        public async void Send(string message, string encoding = "utf-8")
        {
            var sendBytes = Encoding.GetEncoding(encoding).GetBytes(message);
#if NET
            await client.SendAsync(sendBytes, 0);
#else
            client.Send(sendBytes);
#endif
        }

        public void Dispose()
        {
            if (this == null) return;
            client.Shutdown(SocketShutdown.Both);
            client.Disconnect(true);
            client.Close();
            //_socket.Dispose();
        }
    }
}
