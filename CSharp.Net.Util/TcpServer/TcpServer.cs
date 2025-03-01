using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public class TcpServer : IDisposable
    {
        private readonly int _port;
        private TcpListener _listener;
        private bool _isRunning;

        public event EventHandler<SocketEventArgs> OnReceive;

        public TcpServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _isRunning = true;
            Console.WriteLine($"Socket server started on port {_port}");

            Task.Run(async () => await AcceptClientsAsync());
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
#if NET8_0_OR_GREATER
            _listener?.Dispose();
#endif
            if (_listener != null)
                _listener = null;
            Console.WriteLine($"Socket server stoped");
        }

        private async Task AcceptClientsAsync()
        {
            while (_isRunning)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(async () => await HandleClientAsync(client));

            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var buffer = new byte[1024];
            var stream = client.GetStream();

            while (_isRunning)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break; // Client disconnected
                }

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (message == "<EMPTY>")
                    message = ""; // 处理空消息

                var eventArgs = new SocketEventArgs { Message = message };
                OnReceive?.Invoke(this, eventArgs);

                //if (message.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                //{
                //    StopClient();
                //    break;
                //}

                // Send a response back to the client
                var response = Encoding.UTF8.GetBytes(eventArgs.Response ?? "");
                await stream.WriteAsync(response, 0, response.Length);
            }

            client.Close();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
