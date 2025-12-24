using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util.LiteSockets
{
    internal class SocketWire : IWire
    {
        private readonly Socket _socket;
        public SocketWire(Socket socket) => _socket = socket;

        public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct)
         => await _socket.ReceiveAsync(buffer, SocketFlags.None, ct);

        public async ValueTask<int> WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct)
            => await _socket.SendAsync(buffer, SocketFlags.None, ct);

        public ValueTask DisposeAsync()
        {
            try { _socket.Shutdown(SocketShutdown.Both); } catch { }
            _socket.Close();
            _socket.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
