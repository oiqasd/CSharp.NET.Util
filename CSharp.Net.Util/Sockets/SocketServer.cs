using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public class SocketServer : SocketHandleHub, IDisposable
    {
        IPAddress _ipAddress = null;
        const int _port = 80;
        //最大连接数队列
        int Backlog = 512;
        int ReceiveBufferSize = 64 * 1024;
        int SendQueueCapacity = 1024;
        bool NoDelay = true;
        bool KeepAlive = true;
        IPEndPoint ipEndPoint = null;
        Socket _listener = null;
        public event EventHandler<SocketEventArgs> OnReceived;

        public SocketServer(ProtocolType protocolType = ProtocolType.Tcp)
        {
            _listener = new Socket(SocketType.Stream, protocolType);
            //  _listener = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, _protocolType);
        }

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
        public async void Listen(int port = _port) => Listen(null, port);
        public async void Listen(IPAddress? ip = null, int port = _port)
        {
            _cts = new CancellationTokenSource();
            //if (_listener != null)
            //    return;
            if (ipEndPoint == null)
                ipEndPoint = new IPEndPoint(ip ?? IPAddress.Any, port);
            //  _listener = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //new TcpListener

            _listener.Bind(ipEndPoint);
            _listener.Listen(Backlog);

            //Trace.WriteLine("Server is start.");
            //_isRunning = true;

            Task.Run(async () => await AcceptLoopAsync(_cts.Token));
        }



        private async Task AcceptLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                Socket? s = null;
                try
                {
#if NET
                    s = await _listener.AcceptAsync(ct);
#else
                    s = await _listener.AcceptAsync();
#endif
                    //ConfigureSocket(s);

                    //var client = await Task.Factory.FromAsync(_listener.BeginAccept, _listener.EndAccept, null);
                    _ = Task.Run(async () => await HandleClientAsync(s));

                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Hub] Accept error: {ex.Message}");
                    try { s?.Dispose(); } catch { }
                    await Task.Delay(50, ct);
                }

            }
        }

        private void ConfigureSocket(Socket s)
        {
            try { s.NoDelay = NoDelay; } catch { }
            try { s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, KeepAlive); } catch { }
            try { s.ReceiveBufferSize = ReceiveBufferSize; } catch { }
        }

        //public async Task StartListeningAsync()
        //{
        //    while (true)
        //    {
        //        Socket handler = await _listener.AcceptAsync();
        //        _ = Task.Run(async () =>
        //        {
        //            var data = await ReceiveAsync(handler);
        //            var eventArgs = new SocketEventArgs { Message = data };
        //            OnReceived?.Invoke(this, eventArgs);
        //        });
        //    }
        //}

        public async Task HandleClientAsync(Socket handler)
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    //获取客户端的IP和端口
                    var epoint = (handler.RemoteEndPoint as IPEndPoint);
                    var formAddress = epoint.Address.ToString().Replace("::ffff:", "");
                    Trace.WriteLine(formAddress + ":" + epoint.Port + "已连接");
                
                    int port = epoint.Port;
                    var data = await ReceiveAsync(handler);

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
                    OnReceived?.Invoke(this, eventArgs);
                    if (eventArgs.ResponseData.IsHasValue())
                    {
                        await SendDataAsync(handler, eventArgs.ResponseData);
                        //await handler.SendAsync(new ArraySegment<byte>(eventArgs.ResponseData), SocketFlags.None);
                    }
                    else if (eventArgs.Response.IsNotNullOrEmpty())
                    {
                        var response = Encoding.UTF8.GetBytes(eventArgs.Response ?? string.Empty);
                        if (response != null)
                            await SendDataAsync(handler, response);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Socket: {ex.Message}");
            }
            finally
            {
                try { handler.Shutdown(SocketShutdown.Both); } catch { }
                try { handler.Close(); } catch { }
                try { handler.Dispose(); } catch { }
            }
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
        /*
        [Obsolete]
        public async Task<string> ReceiveAndSend(string sendData)
        {
            if (_listener == null) throw new Exception("Listener is not created");
            SocketHandler handler = new SocketHandler(await _listener.AcceptAsync());
            //var epoint = (handler.RemoteEndPoint as IPEndPoint);
            //string formAddress = epoint.Address.ToString();
            //int port = epoint.Port;
            var data = await handler.Receive();// await ReceiveAsync(handler);

            var sendBytes = Encoding.UTF8.GetBytes(sendData);
            handler.Send(sendBytes);
            //handler.Dispose();
            //socketClose(handler);
            return data;
        }
        [Obsolete]
        public async void Send(Socket socket, byte[] sendData)
        {
            if (_listener == null) throw new Exception("Listener is not created");
            SocketHandler handler = new SocketHandler(await _listener.AcceptAsync());

            handler.Send(sendData);
        }
        */

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
            try { _cts?.Cancel(); } catch { }

            if (_listener != null)
            {
                try { _listener.Shutdown(SocketShutdown.Both); } catch { }
                try { _listener.Disconnect(true); } catch { }
                try { _listener.Close(); } catch { }
                try { _listener.Dispose(); } catch { }
                _listener = null;
            }
            ipEndPoint = null;
            _ipAddress = null;

            try { _cts?.Dispose(); } catch { }
            _cts = null;

            GC.SuppressFinalize(this);
        }

        //public async ValueTask DisposeAsync()
        //{
        //    try { _cts?.Cancel(); } catch { }
        //    foreach (var kv in _clients)
        //        await kv.Value.DisposeAsync();

        //    try { _listener.Close(); } catch { }
        //    _cts?.Dispose();
        //}
    }
}
