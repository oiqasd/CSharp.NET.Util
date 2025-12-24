using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers.Binary;
using System.Buffers;
using System.IO;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace CSharp.Net.Util.LiteSockets
{
    /// <summary>
    /// 客户端
    /// 消息格式：[4字节消息总长度][2字节typeId长度][typeId(UTF8)][payload(Json UTF8)]
    /// 所有整数均为 大端序 (BigEndian)。
    /// </summary>
    public sealed class SocketClient : IMessageNode
    {
        private readonly IWire _wire;
        private readonly Channel<byte[]> _sendQ;
        private readonly CancellationTokenSource _cts;
        private readonly Dictionary<string, Func<ReadOnlyMemory<byte>, Task>> _handlers;//消息路由表
        private readonly int _maxFrame;

        /// <summary>
        /// 文件接收示例
        /// <code>
        /// client.OnFileReceived += async (transferId, fileName, bytes) =>
        ///{
        ///   await File.WriteAllBytesAsync("recv_" + fileName, bytes);
        ///   Console.WriteLine($"收到文件 {fileName}, 大小 {bytes.Length} 字节");
        ///};
        /// </code>
        /// </summary>
        public event Func<string, string, byte[], Task>? OnFileReceived;

        /// <summary>
        /// 文件接收缓存
        /// </summary>
        private readonly ConcurrentDictionary<string, SortedDictionary<int, byte[]>> _fileBuffers;

        /// <summary>
        /// 文件传输进度：transferId, fileName, 当前已完成(字节), 总大小(字节)
        /// <code>
        /// client.OnFileProgress += (id, name, done, total) =>
        /// {
        ///    double percent = (double)done / total * 100;
        ///    Console.WriteLine($"发送进度: {name} {percent:F1}% ({done}/{total} bytes)");
        /// };
        /// </code>
        /// </summary>
        public event Action<string, string, long, long>? OnFileProgress;
        /// <summary>
        /// 文件传输取消
        /// </summary>
        private readonly ConcurrentDictionary<string, FileTransferContext> _fileTransfers;
        /// <summary>
        /// 文件传输被取消 (transferId, fileName)
        /// </summary>
        public event Action<string, string>? OnFileCanceled;

        public SocketClient(IWire wire, int sendQueueCapacity = 1024, int maxFrame = 256 * 1024 * 1024)
        {
            _cts = new();
            _handlers = new();
            _fileBuffers = new();
            _fileTransfers = new();
            _wire = wire;
            _maxFrame = maxFrame;
            InitFileReceiver();
            _sendQ = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(sendQueueCapacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });

            _ = SendLoopAsync();
            _ = ReceiveLoopAsync();
        }

        #region 文件传输

        /// <summary>
        /// 文件分块传输协议
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="bytes">文件字节</param>
        /// <param name="chunkSize">每块大小，默认64kb</param>
        /// <param name="maxBytesPerSecond">每秒传输上限，默认1M</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task SendFileAsync(string fileName, byte[] bytes, int chunkSize = 64 * 1024, int maxBytesPerSecond = 1024 * 1024, CancellationToken ct = default)
        {
            int totalChunks = (bytes.Length + chunkSize - 1) / chunkSize;
            string transferId = Guid.NewGuid().ToString();
            int sentBytesThisSecond = 0;
            long totalSent = 0;
            var ctx = new FileTransferContext(transferId, fileName, bytes.Length);
            _fileTransfers[transferId] = ctx;
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ctx.Token, ct);

            var sw = new Stopwatch();
            try
            {
                for (int i = 0; i < totalChunks; i++)
                {
                    linkedCts.Token.ThrowIfCancellationRequested();
                    var chunk = new FileChunk
                    {
                        TransferId = transferId,
                        FileName = fileName,
                        ChunkIndex = i,
                        TotalChunks = totalChunks,
                        Data = bytes.Skip(i * chunkSize).Take(chunkSize).ToArray()
                    };
                    await SendAsync(MessageType.FILE, chunk);

                    ctx.TransferredBytes += chunk.Data.Length;
                    OnFileProgress?.Invoke(transferId, chunk.FileName, totalSent, bytes.Length);

                    sentBytesThisSecond += chunk.Data.Length;
                    // 限流：每秒最多发送 maxBytesPerSecond
                    if (sentBytesThisSecond >= maxBytesPerSecond)
                    {
                        sw.Restart();
                        while (sw.ElapsedMilliseconds < 1000)
                            await Task.Delay(10, linkedCts.Token);
                        sentBytesThisSecond = 0;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"文件传输已取消:{ctx.FileName}");
            }
            finally
            {
                _fileTransfers.TryRemove(transferId, out _);
            }
        }

        /// <summary>
        /// 初始化文件类消息接收
        /// </summary>
        /// <returns></returns>
        async ValueTask InitFileReceiver()
        {
            OnMessage<FileChunk>(MessageType.FILE, async chunk =>
            {
                //if (OnFileReceived != null)
                //    await OnFileReceived(chunk);

                var ctx = _fileTransfers.GetOrAdd(chunk.TransferId,
                    _ => new FileTransferContext(chunk.TransferId, chunk.FileName, (long)chunk.TotalChunks * chunk.Data.Length));

                if (ctx.Token.IsCancellationRequested)
                {
                    Console.WriteLine($"⚠️ 文件接收取消: {ctx.FileName}");
                    _fileTransfers.TryRemove(chunk.TransferId, out _);
                    _fileBuffers.TryRemove(chunk.TransferId, out _); //清理缓存
                    OnFileCanceled?.Invoke(ctx.TransferId, ctx.FileName);
                    return;
                }
                var dict = _fileBuffers.GetOrAdd(chunk.TransferId, _ => new SortedDictionary<int, byte[]>());
                dict[chunk.ChunkIndex] = chunk.Data;

                ctx.TransferredBytes = dict.Values.Sum(a => a.Length);
                OnFileProgress?.Invoke(ctx.TransferId, ctx.FileName, ctx.TransferredBytes, ctx.TotalBytes);

                if (dict.Count == chunk.TotalChunks)
                {
                    // 拼装文件
                    var fileBytes = dict.OrderBy(kv => kv.Key).SelectMany(kv => kv.Value).ToArray();
                    _fileBuffers.TryRemove(chunk.TransferId, out _);
                    _fileTransfers.TryRemove(ctx.TransferId, out _);

                    if (OnFileReceived != null)
                        await OnFileReceived(chunk.TransferId, chunk.FileName, fileBytes);
                }
            });
        }

        public bool CancelFileTransfer(string transferId)
        {
            if (_fileTransfers.TryGetValue(transferId, out var ctx))
            {
                ctx.Cancel();
                _fileBuffers.TryRemove(transferId, out _); // 清理分块缓存
                OnFileCanceled?.Invoke(transferId, ctx.FileName); // 触发取消事件
                return true;
            }
            return false;
        }
        #endregion

        public ValueTask SendAsync<T>(string typeId, T obj, CancellationToken ct = default)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(obj);
            var typeIdBytes = Encoding.UTF8.GetBytes(typeId);
            int frameLen = 2 + typeIdBytes.Length + json.Length;

            byte[] buf = ArrayPool<byte>.Shared.Rent(4 + frameLen);
            BinaryPrimitives.WriteInt32BigEndian(buf.AsSpan(0, 4), frameLen);
            BinaryPrimitives.WriteUInt16BigEndian(buf.AsSpan(4, 2), (ushort)typeIdBytes.Length);
            typeIdBytes.CopyTo(buf.AsSpan(6));
            json.CopyTo(buf.AsSpan(6 + typeIdBytes.Length));

            _ = _sendQ.Writer.WriteAsync(buf, ct);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// 字串消息接收
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeId"></param>
        /// <param name="handler"></param>
        public void OnMessage<T>(string typeId, Func<T, Task> handler)
        {
            _handlers[typeId] = async mem =>
            {
                var obj = JsonSerializer.Deserialize<T>(mem.Span);
                if (obj != null) await handler(obj);
            };
        }

        private async Task SendLoopAsync()
        {
            try
            {
                while (await _sendQ.Reader.WaitToReadAsync(_cts.Token))
                {
                    while (_sendQ.Reader.TryRead(out var packet))
                    {
                        await _wire.WriteAsync(packet, _cts.Token);
                        ArrayPool<byte>.Shared.Return(packet);
                    }
                }
            }
            catch { }
        }

        private async Task ReceiveLoopAsync()
        {
            var header = ArrayPool<byte>.Shared.Rent(4);

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    await ReadExactAsync(header.AsMemory(0, 4), _cts.Token);
                    int frameLen = BinaryPrimitives.ReadInt32BigEndian(header);
                    if (frameLen <= 0 || frameLen > _maxFrame) throw new IOException();

                    var frame = ArrayPool<byte>.Shared.Rent(frameLen);
                    await ReadExactAsync(frame.AsMemory(0, frameLen), _cts.Token);

                    int typeLen = BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(0, 2));
                    string typeId = Encoding.UTF8.GetString(frame, 2, typeLen);
                    var payload = new ReadOnlyMemory<byte>(frame, 2 + typeLen, frameLen - 2 - typeLen);

                    if (_handlers.TryGetValue(typeId, out var h))
                        await h(payload);

                    ArrayPool<byte>.Shared.Return(frame);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(header);
            }
        }

        private async Task ReadExactAsync(Memory<byte> dst, CancellationToken ct)
        {
            int got = 0;
            while (got < dst.Length)
            {
                int n = await _wire.ReadAsync(dst.Slice(got), ct);
                if (n <= 0) throw new IOException();
                got += n;
            }
        }

        public async ValueTask DisposeAsync()
        {
            try { _cts.Cancel(); } catch { }
            _sendQ.Writer.TryComplete();
            await _wire.DisposeAsync();
        }
    }
}
