using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public class TcpHelper : IDisposable
    {
        TcpClient _tcpClient;
        NetworkStream _stream;
        string _server;
        int _port;
        bool _isRunning;
        public TcpHelper(string server, int port)
        {
            _port = port;
            _server = server;
        }
        public void StartClient()
        {
            _tcpClient.ConnectAsync(_server, _port).GetAwaiter();
            Console.WriteLine("Connected to server");
            _stream = _tcpClient.GetStream();
            _isRunning = true;
        }
        public void StopClient()
        {
            _isRunning = false;
            _stream.Close();
            _tcpClient.Close();
            _tcpClient.Dispose();
            _tcpClient = null;
            Console.WriteLine("Client closed");
        }
        public async Task SendAsync(string message)
        {
            if (!_isRunning) throw new InvalidOperationException("Client is not running");
            if (message.IsNullOrEmpty()) message = "<EMPTY>";
            var buffer = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public async Task<string> ReadAsync(int dataLength = 256)
        {
            if (!_isRunning) throw new InvalidOperationException("Client is not running");
            byte[] data = new byte[dataLength];
            int bytes = await _stream.ReadAsync(data, 0, data.Length);
            string responseData = Encoding.UTF8.GetString(data, 0, bytes);
            return responseData;
        }

        public void Dispose()
        {
            StopClient();
        }
    }
}
