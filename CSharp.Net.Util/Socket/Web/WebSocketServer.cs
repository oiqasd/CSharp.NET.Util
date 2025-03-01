using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 建立webScoket服务端
    /// 查找端口占用 netstat -ano | findstr 8080
    /// </summary>
    public class WebSocketServer : IDisposable
    {
        private readonly int _port;
        private HttpListener _listener;
        private bool _isRunning;

        public event EventHandler<SocketEventArgs> OnReceive;

        public WebSocketServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://*:{_port}/");
            _listener.Start();
            _isRunning = true;
            Console.WriteLine($"WebSocket server started on port {_port}");

            Task.Run(async () => await AcceptClientsAsync());
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
            Console.WriteLine($"WebSocket server stoped");
        }

        private async Task AcceptClientsAsync()
        {
            while (_isRunning)
            {
                var context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    _ = Task.Run(async () => await HandleClientAsync(wsContext.WebSocket));
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async Task HandleClientAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024];

            while (_isRunning && webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Received from client: " + message);

                    var eventArgs = new SocketEventArgs { Message = message };
                    OnReceive?.Invoke(this, eventArgs);

                    var response = Encoding.UTF8.GetBytes(eventArgs.Response);
                    await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
