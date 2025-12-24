using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    sealed class ClientConnection : IAsyncDisposable
    {
        private readonly Socket _socket;
        private readonly Channel<byte[]> _sendQ;
        private readonly int _recvBufSize;
        private readonly Guid _id;
        private readonly Action<Guid, ReadOnlyMemory<byte>> _onMessage;
        private readonly Action<Guid, EndPoint, Exception?> _onClosed;

        private readonly CancellationTokenSource _cts = new();
        private Task? _recvTask;
        private Task? _sendTask;

        public ClientConnection(Guid id, Socket socket, int recvBufSize, int sendQCapacity,
            Action<Guid, ReadOnlyMemory<byte>> onMessage,
            Action<Guid, EndPoint, Exception?> onClosed)
        {
            _id = id;
            _socket = socket;
            _recvBufSize = recvBufSize;
            _onMessage = onMessage;
            _onClosed = onClosed;

            // 有界队列：背压。满时 EnqueueAsync 会等待
            _sendQ = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(sendQCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
        }

        public void Start()
        {
            _recvTask = Task.Run(ReceiveLoopAsync);
            _sendTask = Task.Run(SendLoopAsync);
        }

        public async ValueTask EnqueueAsync(ReadOnlyMemory<byte> payload, CancellationToken ct)
        {
            // 帧化：长度 + 数据（大端）
            var packet = new byte[4 + payload.Length];
            BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(0, 4), payload.Length);
            payload.Span.CopyTo(packet.AsSpan(4));

            await _sendQ.Writer.WriteAsync(packet, ct);
        }

        public async ValueTask DisposeAsync()
        {
            try { _cts.Cancel(); } catch { }
            _sendQ.Writer.TryComplete();

            try { _socket.Shutdown(SocketShutdown.Both); } catch { }
            try { _socket.Close(); } catch { }

            if (_recvTask is not null) await Task.WhenAny(_recvTask, Task.Delay(200));
            if (_sendTask is not null) await Task.WhenAny(_sendTask, Task.Delay(200));

            _cts.Dispose();
            _socket.Dispose();
        }

        private async Task SendLoopAsync()
        {
            try
            {
                while (await _sendQ.Reader.WaitToReadAsync(_cts.Token))
                {
                    while (_sendQ.Reader.TryRead(out var packet))
                        await SendAllAsync(packet, _cts.Token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"[SendLoop {_id}] {ex.Message}");
            }
            finally
            {
                _onClosed(_id, _socket.RemoteEndPoint!, null);
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var header = new byte[4];
            var buf = new byte[_recvBufSize];

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    // 读长度
                    await ReceiveExactAsync(header, 4, _cts.Token);
                    int len = BinaryPrimitives.ReadInt32BigEndian(header);

                    if (len < 0 || len > 256 * 1024 * 1024) // 防御过大报文
                        throw new InvalidDataException($"Invalid length: {len}");

                    var payload = new byte[len];
                    int remaining = len;
                    int offset = 0;

                    // 大包分段读，避免临时大数组也可以用 ArrayPool 优化
                    while (remaining > 0)
                    {
                        int toRead = Math.Min(remaining, buf.Length);
                        int n = await _socket.ReceiveAsync(buf.AsMemory(0, toRead), SocketFlags.None, _cts.Token);
                        if (n <= 0) throw new SocketException((int)SocketError.ConnectionReset);
                        new ReadOnlySpan<byte>(buf, 0, n).CopyTo(payload.AsSpan(offset));
                        offset += n;
                        remaining -= n;
                    }

                    _onMessage(_id, payload);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _onClosed(_id, _socket.RemoteEndPoint!, ex);
            }
        }

        private async Task SendAllAsync(byte[] data, CancellationToken ct)
        {
            int sent = 0;
            while (sent < data.Length)
            {
                int n = await _socket.SendAsync(data.AsMemory(sent), SocketFlags.None, ct);
                if (n <= 0) throw new SocketException((int)SocketError.ConnectionReset);
                sent += n;
            }
        }

        private async Task ReceiveExactAsync(byte[] buf, int count, CancellationToken ct)
        {
            int got = 0;
            while (got < count)
            {
                int n = await _socket.ReceiveAsync(buf.AsMemory(got, count - got), SocketFlags.None, ct);
                if (n <= 0) throw new SocketException((int)SocketError.ConnectionReset);
                got += n;
            }
        }
    }
}
