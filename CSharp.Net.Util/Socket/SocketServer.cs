using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public class SocketServer : IDisposable
    {
        IPAddress _ipAddress = null;
        const int _port = 80;
        //最大连接数队列
        int _maxQueues = 10;
        IPEndPoint ipEndPoint = null;
        Socket _listener = null;
        private bool _isRunning;
        /*
        public SocketServer(int port)
        {
            _port = port;
            ipEndPoint = new IPEndPoint(IPAddress.Any, _port);
            //listen();
        }
        public SocketServer(int port, int maxQueues)
        {
            _port = port;
            _maxQueues = maxQueues;
            ipEndPoint = new IPEndPoint(IPAddress.Any, _port);
            //listen();
        }

        public SocketServer(string hostOrAddress, int port = 80)
        {
            _port = port;
            IPHostEntry ipHostInfo = Dns.GetHostEntry(hostOrAddress);
            _ipAddress = ipHostInfo.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            //listen();
        }*/
        //public Socket ListenHost(string hostOrAddress, int port = 80)
        //{
        //    if (_ipAddress != null)
        //        throw new Exception("Server client is created");
        //    IPHostEntry ipHostInfo = Dns.GetHostEntry(hostOrAddress);
        //    _ipAddress =  ipHostInfo.AddressList[0];
        //    _port = port > 0 ? port : _port;
        //    return listen();
        //}
        //public Socket ListenIP(string ip, int port = 80)
        //{
        //    if (_ipAddress != null)
        //        throw new Exception("Server client is created");
        //    _ipAddress = IPAddress.Parse(ip);
        //    _port = port > 0 ? port : _port;
        //    return listen();
        //}

        public void Listen(int port = _port)
        {
            if (_listener != null)
                return;
            if (ipEndPoint == null)
                ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            _listener = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //new TcpListener

            _listener.Bind(ipEndPoint);
            _listener.Listen(_maxQueues);
            //Trace.WriteLine("Server is start.");
            _isRunning = true;
            Task.Run(async () => await AcceptConnections());
        }

        public event EventHandler<SocketEventArgs> OnReceive;

        private async Task AcceptConnections()
        {
            while (_isRunning)
            {
                Socket handler = await _listener.AcceptAsync();
                _ = Task.Run(async () => await HandleConnection(handler));
            }
        }
        //public async Task StartListeningAsync()
        //{
        //    while (true)
        //    {
        //        Socket handler = await _listener.AcceptAsync();
        //        _ = Task.Run(async () =>
        //        {
        //            var data = await ReceiveSocketData(handler);
        //            var eventArgs = new SocketEventArgs { Message = data };
        //            OnReceive?.Invoke(this, eventArgs);
        //        });
        //    }
        //}

        public async Task HandleConnection(Socket handler)
        {
            try
            {
                while (_isRunning)
                {
                    //获取客户端的IP和端口
                    var epoint = (handler.RemoteEndPoint as IPEndPoint);
                    Trace.WriteLine(epoint.Address + ":" + epoint.Port + "已连接");
                    string formAddress = epoint.Address.ToString();
                    int port = epoint.Port;

                    var data = await ReceiveSocketData(handler);                    
                    // break;
                    //var eom = "<|EOM|>";
                    //if (response.IndexOf(eom) > -1 /* is end of message */)
                    //{
                    //Trace.WriteLine($"received {handler.RemoteEndPoint.ToString()} message: \"{response.Replace(eom, "")}\"");
                    //var ackMessage = "<|ACK|>";
                    //var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    //await handler.SendAsync(echoBytes, 0);
                    //Trace.WriteLine($"Socket server sent acknowledgment: \"{ackMessage}\"");
                    //break;
                    //}
                    //socketClose(handler);

                    var eventArgs = new SocketEventArgs { ReceivedData = data, Address = formAddress + ":" + port };
                    OnReceive?.Invoke(this, eventArgs);
                    if (eventArgs.ResponseData.IsHasValue())
                    {
                        await handler.SendAsync(new ArraySegment<byte>(eventArgs.ResponseData), SocketFlags.None);
                    }
                    else if (eventArgs.Response.IsNotNullOrEmpty())
                    {
                        var response = Encoding.UTF8.GetBytes(eventArgs.Response ?? string.Empty);
                        if (response != null)
                            await handler.SendAsync(new ArraySegment<byte>(response), SocketFlags.None);
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket: {ex.Message}");
            }
            finally
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }

        public async Task<string> ReceiveAndSend(string sendData)
        {
            if (_listener == null) throw new Exception("Listener is not created");
            SocketHandler handler = new SocketHandler(await _listener.AcceptAsync());
            //var epoint = (handler.RemoteEndPoint as IPEndPoint);
            //string formAddress = epoint.Address.ToString();
            //int port = epoint.Port;
            var data = await handler.Receive();// await ReceiveSocketData(handler);

            //var sendBytes = Encoding.UTF8.GetBytes(sendData);
#if NET
            handler.Send(sendData);
#else
            handler.Send(sendData);
#endif
            //handler.Dispose();
            //socketClose(handler);
            return data;
        }
        public async void Send(string sendData)
        {
            if (_listener == null) throw new Exception("Listener is not created");
            SocketHandler handler = new SocketHandler(await _listener.AcceptAsync());

            handler.Send(sendData);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        async Task<byte[]> ReceiveSocketData(Socket socket)
        {
            if (socket == null) return null;
            var buffer = new byte[1_024]; //每次接收1KB的数据
            int received = 0;
            using (var ms = new MemoryStream())
            {
                do
                {
                    received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (received > 0)
                    {
                        ms.Write(buffer, 0, received);
                    }
                } while (received == buffer.Length); // 当接收到的数据小于缓冲区大小时，说明消息接收完毕

                return ms.ToArray();
            }
            /*
          string data = "";
#if NET
            do
            {
                received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (received == 0)
                    return null;
                data += Encoding.UTF8.GetString(buffer, 0, received);
            } while (received == buffer.Length);
#else
            received = socket.Receive(buffer, SocketFlags.None);
             if (received == 0)
                return null;
            data = Encoding.UTF8.GetString(buffer, 0, received);
#endif
            return data;*/
        }

        public bool IsImage(byte[] buffer)
        {
            if (buffer == null) return false;
            // 检查JPEG文件头
            if (buffer.Length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xD8)
            {
                return true;
            }
            // 检查PNG文件头
            if (buffer.Length >= 8 && buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
            {
                return true;
            }
            // 可以添加更多图片格式的判断
            return false;
        }

        /// <summary>
        /// 释放会话
        /// </summary>
        /// <param name="socket"></param>
        async void socketClose(Socket socket)
        {
            if (socket == null) return;
            socket.Shutdown(SocketShutdown.Both);
#if NET
            await socket.DisconnectAsync(true);
#else
            socket.Disconnect(true);
#endif
            socket.Close();
            socket.Dispose();
        }

        public void Dispose()
        {
            _isRunning = false;
            if (_listener != null)
            {
                _listener.Shutdown(SocketShutdown.Both);
                _listener.Disconnect(true);
                _listener.Close();
                _listener.Dispose();
            }
            if (ipEndPoint != null)
                ipEndPoint = null;
            if (_ipAddress != null)
                _ipAddress = null;
        }
    }
}
