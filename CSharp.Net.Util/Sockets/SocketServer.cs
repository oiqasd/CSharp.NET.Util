using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public class SocketServer : SocketHandleHub, IDisposable
    {
        private const int DefaultPort = 80;
        private readonly object _lifecycleLock = new object();
        private readonly ProtocolType _protocolType;
        private readonly ConcurrentDictionary<Socket, Task> _clientTasks = new ConcurrentDictionary<Socket, Task>();
        private Socket _listener;
        private Task _acceptTask;
        private bool _disposed;

        public SocketServer(ProtocolType protocolType = ProtocolType.Tcp)
            : this(protocolType, DefaultMaxPacketSize)
        {
        }

        public SocketServer(ProtocolType protocolType, int maxPacketSize)
            : base(maxPacketSize)
        {
            _protocolType = protocolType;
        }

        //最大连接数队列
        public int Backlog { get; set; } = 512;
        public int ReceiveBufferSize { get; set; } = 64 * 1024;
        public bool NoDelay { get; set; } = true;
        public bool KeepAlive { get; set; } = true;
        public event EventHandler<SocketEventArgs> OnReceived;

        /// <summary>
        /// 监听循环的生命周期任务；服务器停止后完成。
        /// </summary>
        public Task Completion
        {
            get
            {
                lock (_lifecycleLock)
                    return _acceptTask ?? Task.CompletedTask;
            }
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
        }

        public Socket ListenHost(string hostOrAddress, int port = 80)
        {
            if (_ipAddress != null)
                throw new Exception("Server client is created");
            IPHostEntry ipHostInfo = Dns.GetHostEntry(hostOrAddress);
            _ipAddress =  ipHostInfo.AddressList[0];
            _port = port > 0 ? port : _port;
            return listen();
        }
        public Socket ListenIP(string ip, int port = 80)
        {
            if (_ipAddress != null)
                throw new Exception("Server client is created");
            _ipAddress = IPAddress.Parse(ip);
            _port = port > 0 ? port : _port;
            return listen();
        }
        */

        public Task Listen(int port = DefaultPort) =>
            Listen(null, port, CancellationToken.None);

        public Task Listen(int port, CancellationToken cancellationToken) =>
            Listen(null, port, cancellationToken);

        public Task Listen(IPAddress? ip = null, int port = DefaultPort) =>
            Listen(ip, port, CancellationToken.None);

        public Task Listen(IPAddress? ip, int port, CancellationToken cancellationToken)
        {
            lock (_lifecycleLock)
            {
                ThrowIfDisposed();
                if (_acceptTask != null)
                    throw new InvalidOperationException("The socket server has already started.");
                if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                    throw new ArgumentOutOfRangeException(nameof(port));

                var endpoint = new IPEndPoint(ip ?? IPAddress.Any, port);
                _listener = new Socket(endpoint.AddressFamily, SocketType.Stream, _protocolType);
                _listener.Bind(endpoint);
                _listener.Listen(Backlog);
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _acceptTask = AcceptLoopAsync(_cts.Token);
                return _acceptTask;
            }
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(CloseListener))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Socket client = null;
                    try
                    {
#if NET
                        client = await _listener.AcceptAsync(cancellationToken).ConfigureAwait(false);
#else
                        client = await _listener.AcceptAsync().ConfigureAwait(false);
                        cancellationToken.ThrowIfCancellationRequested();
#endif
                        ConfigureSocket(client);
                        Task task = HandleClientAsync(client, cancellationToken);
                        _clientTasks.TryAdd(client, task);
                        _ = task.ContinueWith(
                            completedTask => CompleteClientTask(client, completedTask),
                            CancellationToken.None,
                            TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Default);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        client?.Dispose();
                        break;
                    }
                    catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                    {
                        client?.Dispose();
                        break;
                    }
                    catch (SocketException) when (cancellationToken.IsCancellationRequested)
                    {
                        client?.Dispose();
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Hub] Accept error: {ex.Message}");
                        client?.Dispose();
                        try
                        {
                            await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void CompleteClientTask(Socket client, Task completedTask)
        {
            _clientTasks.TryRemove(client, out Task ignored);
            if (completedTask.IsFaulted)
            {
                Exception exception = completedTask.Exception;
                Trace.WriteLine($"Socket client task error: {exception}");
            }
        }

        private void ConfigureSocket(Socket socket)
        {
            // ConfigureSocket(s);
            if (_protocolType == ProtocolType.Tcp)
            {
                try { socket.NoDelay = NoDelay; } catch { }
            }
            try { socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, KeepAlive); } catch { }
            try { socket.ReceiveBufferSize = ReceiveBufferSize; } catch { }
        }

        /*
        public async Task StartListeningAsync()
        {
            while (true)
            {
                Socket handler = await _listener.AcceptAsync();
                _ = Task.Run(async () =>
                {
                    var data = await ReceiveAsync(handler);
                    var eventArgs = new SocketEventArgs { Message = data };
                    OnReceived?.Invoke(this, eventArgs);
                });
            }
        }
        */

        public async Task HandleClientAsync(Socket handler) =>
            await HandleClientAsync(handler, _cts?.Token ?? CancellationToken.None).ConfigureAwait(false);

        public async Task HandleClientAsync(Socket handler, CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(() => CloseSocket(handler)))
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        //获取客户端的IP和端口
                        var endpoint = handler.RemoteEndPoint as IPEndPoint;
                        string formAddress = endpoint?.Address.ToString().Replace("::ffff:", string.Empty) ?? string.Empty;
                        int port = endpoint?.Port ?? 0;
                        Trace.WriteLine(formAddress + ":" + port + "已连接");

                        byte[] data = await ReceiveAsync(handler, cancellationToken).ConfigureAwait(false);

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

                        var eventArgs = new SocketEventArgs
                        {
                            ReceivedData = data,
                            Address = formAddress + ":" + port
                        };
                        OnReceived?.Invoke(this, eventArgs);
                        if (eventArgs.ResponseData.IsHasValue())
                        {
                            await SendDataAsync(handler, eventArgs.ResponseData, cancellationToken).ConfigureAwait(false);
                            //await handler.SendAsync(new ArraySegment<byte>(eventArgs.ResponseData), SocketFlags.None);
                        }
                        else if (eventArgs.Response.IsNotNullOrEmpty())
                        {
                            var response = Encoding.UTF8.GetBytes(eventArgs.Response ?? string.Empty);
                            if (response != null)
                                await SendDataAsync(handler, response, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Socket: {ex.Message}");
                    Trace.WriteLine(ex);
                }
                finally
                {
                    CloseSocket(handler);
                }
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
        */

        public async Task StopAsync()
        {
            Task acceptTask;
            CancellationTokenSource cts;
            lock (_lifecycleLock)
            {
                cts = _cts;
                acceptTask = _acceptTask;
            }

            if (cts == null)
                return;

            cts.Cancel();
            CloseListener();
            foreach (Socket socket in _clientTasks.Keys)
                CloseSocket(socket);

            if (acceptTask != null)
            {
                try { await acceptTask.ConfigureAwait(false); }
                catch (OperationCanceledException) { }
            }

            Task[] clientTasks = _clientTasks.Values.ToArray();
            if (clientTasks.Length > 0)
                await Task.WhenAll(clientTasks).ConfigureAwait(false);
        }

        private void CloseListener()
        {
            try { _listener?.Close(); } catch { }
        }

        private static void CloseSocket(Socket socket)
        {
            if (socket == null)
                return;
            try { socket.Shutdown(SocketShutdown.Both); } catch { }
            try { socket.Close(); } catch { }
            try { socket.Dispose(); } catch { }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SocketServer));
        }

        public void Dispose()
        {
            lock (_lifecycleLock)
            {
                if (_disposed)
                    return;
                _disposed = true;
            }

            StopAsync().GetAwaiter().GetResult();
            _cts?.Dispose();
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
