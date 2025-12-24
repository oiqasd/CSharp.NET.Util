using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 客户端
    /// </summary>
    public class SocketClient : SocketHandleHub, IDisposable
    {
        Socket client = null;

        public SocketClient(ProtocolType protocolType = ProtocolType.Tcp)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
        }


        public bool Connect(string host, int port = 80)
        {
            try
            {
                _cts = new CancellationTokenSource();
                IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
                var ipAddress = ipHostInfo.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                client.Connect(ipAddress, port);
                return client.Connected;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                //client.Shutdown(SocketShutdown.Both);
                //client.Dispose();
                //client = null;
                Dispose();
                return false;
            }
        }

        /*
        /// <summary>
        /// 同步发送消息
        /// </summary>
        /// <param name="message"></param>
        public void Send(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            SendBytes(client, messageBytes);
        }
        */

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data"></param>
        public async Task<string> SendAndReceived(string data) => await SendAndReceived(Encoding.UTF8.GetBytes(data));
        //public async Task SendAndReceived(string data) =>  SendBytes(Encoding.UTF8.GetBytes(data));

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data"></param>
        public async Task<string> SendAndReceived(byte[] data)
        {
            await SendDataAsync(client, data);
            var msg = await Receive();
            //Console.WriteLine($"send:{data.Length / 1024}kb,recevie:{msg}");
            return msg;
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data"></param>
        public async Task SendDataAsync(string data)
        {
            await SendDataAsync(client, data);
        }

        public async Task<string> Receive()
        {
            var bytes = await ReceiveAsync(client);
            var message = Encoding.UTF8.GetString(bytes);
            return message;
        }

        public void StopReceiving()
        {
            _cts?.Cancel();
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
                _cts?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
