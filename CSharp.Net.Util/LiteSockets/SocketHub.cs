using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util.LiteSockets
{
    /// <summary>
    /// 服务端
    /// 与客户端使用相同协议
    /// </summary>
    public sealed class SocketHub : IAsyncDisposable
    {
        private readonly Socket _listener;
        private readonly ConcurrentDictionary<Guid, SocketClient> _clients = new();//连接管理
        private readonly CancellationTokenSource _cts = new();

        public SocketHub(int port, ProtocolType protocolType = ProtocolType.Tcp, int backlog = 512)
        {
            _listener = new Socket(SocketType.Stream, protocolType);
            _listener.Bind(new IPEndPoint(IPAddress.Any, port));
            _listener.Listen(backlog);
        }

        public event Action<Guid, EndPoint>? OnClientConnected;
        public event Action<Guid, EndPoint>? OnClientDisconnected;
        public async Task StartAsync()
        {
            _ = AcceptLoopAsync();
            await Task.CompletedTask;
        }
        public async ValueTask DisposeAsync()
        {
            try { _cts.Cancel(); } catch { }
            foreach (var c in _clients.Values)
                await c.DisposeAsync();
            _listener.Close();
        }

        public ValueTask SendAsync<T>(Guid clientId, string typeId, T obj)
       => _clients.TryGetValue(clientId, out var cli) ? cli.SendAsync(typeId, obj) : ValueTask.CompletedTask;

        public async Task BroadcastAsync<T>(string typeId, T obj, Guid? except = null)
        {
            foreach (var (id, cli) in _clients)
            {
                if (except.HasValue && id == except.Value) continue;
                await cli.SendAsync(typeId, obj);
            }
        }

        private async Task AcceptLoopAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var s = await _listener.AcceptAsync(_cts.Token);
                    var id = Guid.NewGuid();
                    var client = new SocketClient(new SocketWire(s));

                    if (_clients.TryAdd(id, client))
                    {
                        OnClientConnected?.Invoke(id, s.RemoteEndPoint!);
                        client.OnMessage<object>("ping", async _ => await client.SendAsync("pong", new { ts = DateTime.UtcNow }));
                    }
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine(ex.ToString());
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    break;
                }
                finally
                {
                    Console.WriteLine("exit");    
                }
            }
        }

    }
}
