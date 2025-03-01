using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public class SocketClient : IDisposable
    {
        //TcpClient client = null;
        Socket client = null;
        CancellationTokenSource cts = new CancellationTokenSource();
        public event EventHandler<SocketEventArgs> OnReceive;

        public SocketClient()
        {

        }


        public bool Connect(string host, int port = 80)
        {
            try
            {
                if (client == null)
                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
                var ipAddress = ipHostInfo.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                client.Connect(ipAddress, port);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                client = null;
                return false;
            }
        }
        //public TcpClient ConnectIP(string ip, int port)
        //{
        //    if (client != null)
        //        throw new Exception("Client is created");
        //    client = new TcpClient();
        //    client.Client.Connect(IPAddress.Parse(ip), port);
        //    return client;
        //}


        public void Send(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var count = client.Send(messageBytes, SocketFlags.None);
            Console.WriteLine($"{DateTime.Now.ToString(1)} 发送了{count}个字节，应发送{messageBytes.Length}个，{message}");
        }
        public void SendBytes(byte[] bytes)
        {
            var count = client.Send(bytes, SocketFlags.None);
            Console.WriteLine($"{DateTime.Now.ToString(1)} 发送了{count}个字节，应发送{bytes.Length}个");
        }

        public string Receive()
        {
            var buffer = new byte[1_024];
            var received = client.Receive(buffer, SocketFlags.None);
            var message = Encoding.UTF8.GetString(buffer, 0, received);
            return message;
        }

        public async Task StartReceivingAsync(Action<string> onMessageReceived)
        {
            await Task.Run(() =>
            {
                var buffer = new byte[1_024];
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var received = client.Receive(buffer, SocketFlags.None);
                        if (received > 0)
                        {
                            var message = Encoding.UTF8.GetString(buffer, 0, received);
                            onMessageReceived?.Invoke(message);
                        }
                    }
                    catch (SocketException ex)
                    {
                        // Handle socket exception
                        Console.WriteLine($"Socket exception: {ex.Message}");
                        break;
                    }
                }
            }, cts.Token);
        }
        public void StopReceiving()
        {
            cts.Cancel();
        }
        public void Dispose()
        {
            try
            {
                StopReceiving();
                if (client != null)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Disconnect(true);
                    client.Close();
                    client.Dispose();
                    client = null;
                }
            }
            finally
            {
                if (client != null) client = null;
            }
        }
    }
}
