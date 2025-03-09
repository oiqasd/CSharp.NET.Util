using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        /// <summary>
        /// 直接发送消息
        /// </summary>
        /// <param name="message"></param>
        public void Send(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            SendBytes(messageBytes);
        }
        /// <summary>
        /// 直接发送消息
        /// </summary>
        /// <param name="bytes"></param>
        /// <exception cref="Exception"></exception>
        public void SendBytes(byte[] bytes)
        {
            int totalSent = 0;
            while (totalSent < bytes.Length)
            {
                int sent = client.Send(bytes, totalSent, bytes.Length - totalSent, SocketFlags.None);
                if (sent == 0) throw new Exception("发送失败，连接可能已断开");
                totalSent += sent;
            }
            //var count = client.Send(bytes, SocketFlags.None);
            Console.WriteLine($"{DateTime.Now.ToString(1)} 发送了{totalSent}个字节，应发送{bytes.Length}个");
        }
        /// <summary>
        /// 消息头 + 长度 + 数据
        /// </summary>
        /// <param name="data"></param>
        public async Task SendPacket(string data) => await SendPacket(Encoding.UTF8.GetBytes(data));

        /// <summary>
        /// 消息头 + 长度 + 数据
        /// </summary>
        /// <param name="data"></param>
        public async Task SendPacket(byte[] data)
        {
            byte[] header = Encoding.UTF8.GetBytes("MSG_"); // 4字节的消息头
            byte[] lengthBytes = BitConverter.GetBytes(data.Length); // 4字节的数据长度

            //发送 头部 + 长度 + 数据
            await client.SendAsync(new ArraySegment<byte>(header), SocketFlags.None);
            await client.SendAsync(new ArraySegment<byte>(lengthBytes), SocketFlags.None);
            await client.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
        }

        public string Receive()
        {
            var buffer = new byte[1_024];
            int received = 0;
            using (var ms = new MemoryStream())
            {
                do
                {
                    received = client.Receive(buffer, SocketFlags.None);
                    if (received > 0)
                    {
                        ms.Write(buffer, 0, received);
                    }
                } while (received == buffer.Length);

                var totalReceived = ms.ToArray();
                var message = Encoding.UTF8.GetString(totalReceived, 0, received);
                return message;
            }
            //var received = client.Receive(buffer, SocketFlags.None);
            //var message = Encoding.UTF8.GetString(buffer, 0, received);
            //return message;
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
