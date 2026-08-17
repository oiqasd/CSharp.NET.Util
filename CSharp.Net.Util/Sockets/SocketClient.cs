using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 客户端
    /// </summary>
    public class SocketClient : SocketHandleHub, IDisposable
    {
        private readonly object _lifecycleLock = new object();
        private readonly Socket _client;
        private readonly ConcurrentDictionary<int, Task> _operations = new ConcurrentDictionary<int, Task>();
        private readonly SemaphoreSlim _requestLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _receiveLock = new SemaphoreSlim(1, 1);
        private int _nextOperationId;
        private bool _stopping;
        private bool _disposed;

        public SocketClient(ProtocolType protocolType = ProtocolType.Tcp)
            : this(protocolType, DefaultMaxPacketSize)
        {
        }

        public SocketClient(ProtocolType protocolType, int maxPacketSize)
            : base(maxPacketSize)
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
            _cts = new CancellationTokenSource();
        }

        public bool Connect(string host, int port = 80) =>
            ConnectAsync(host, port).GetAwaiter().GetResult();

        public Task<bool> ConnectAsync(string host, int port = 80, CancellationToken cancellationToken = default) =>
            TrackOperation(() => ConnectCoreAsync(host, port, cancellationToken));

        private async Task<bool> ConnectCoreAsync(string host, int port, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host is required.", nameof(host));
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(port));

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken))
            {
                CancellationToken token = linkedCts.Token;
                try
                {
#if NET
                    IPAddress[] addresses = await Dns.GetHostAddressesAsync(host, token).ConfigureAwait(false);
#else
                    using (token.Register(CloseSocket))
                    {
                        IPAddress[] addresses = (await Dns.GetHostEntryAsync(host).ConfigureAwait(false)).AddressList;
                        token.ThrowIfCancellationRequested();
                        return await ConnectToFirstIPv4Async(addresses, port, token).ConfigureAwait(false);
                    }
#endif
#if NET
                    return await ConnectToFirstIPv4Async(addresses, port, token).ConfigureAwait(false);
#endif
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    CloseSocket();
                    return false;
                }
            }
        }

        private async Task<bool> ConnectToFirstIPv4Async(IPAddress[] addresses, int port, CancellationToken cancellationToken)
        {
            IPAddress address = addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            if (address == null)
                throw new SocketException((int)SocketError.HostNotFound);
#if NET
            await _client.ConnectAsync(address, port, cancellationToken).ConfigureAwait(false);
#else
            await _client.ConnectAsync(address, port).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
#endif
            return _client.Connected;
        }

        /*
        /// <summary>
        /// 同步发送消息
        /// </summary>
        /// <param name="message"></param>
        public void Send(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            SendBytes(client, messageBytes);
        }
        */

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data"></param>
        public Task<string> SendAndReceived(string data) =>
            SendAndReceived(Encoding.UTF8.GetBytes(data), CancellationToken.None);

        public Task<string> SendAndReceived(string data, CancellationToken cancellationToken) =>
            SendAndReceived(Encoding.UTF8.GetBytes(data), cancellationToken);

        //public async Task SendAndReceived(string data) => SendBytes(Encoding.UTF8.GetBytes(data));

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data"></param>
        public Task<string> SendAndReceived(byte[] data) =>
            SendAndReceived(data, CancellationToken.None);

        public Task<string> SendAndReceived(byte[] data, CancellationToken cancellationToken) =>
            ReturnWithCallerCancellation(
                TrackOperation(() => SendAndReceiveCoreAsync(data, cancellationToken)),
                cancellationToken);

        private async Task<string> SendAndReceiveCoreAsync(byte[] data, CancellationToken cancellationToken)
        {
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken))
            {
                CancellationToken token = linkedCts.Token;
#if !NET
                using (token.Register(CloseSocket))
#endif
                {
                    await _requestLock.WaitAsync(token).ConfigureAwait(false);
                    try
                    {
                        await _receiveLock.WaitAsync(_cts.Token).ConfigureAwait(false);
                        try
                        {
                            await SendCoreAsync(data, _cts.Token, _cts.Token).ConfigureAwait(false);
                            byte[] bytes = await ReceiveAsync(_client, _cts.Token).ConfigureAwait(false);
                            return Encoding.UTF8.GetString(bytes);
                        }
                        finally
                        {
                            _receiveLock.Release();
                        }
                    }
                    finally
                    {
                        _requestLock.Release();
                    }
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data"></param>
        public Task SendDataAsync(string data) =>
            SendDataAsync(data, CancellationToken.None);

        public Task SendDataAsync(string data, CancellationToken cancellationToken) =>
            ReturnWithCallerCancellation(TrackOperation(async () =>
            {
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken))
                {
#if !NET
                    using (linkedCts.Token.Register(CloseSocket))
#endif
                    {
                        await SendCoreAsync(
                            Encoding.UTF8.GetBytes(data), linkedCts.Token, _cts.Token).ConfigureAwait(false);
                    }
                }
            }), cancellationToken);

        private async Task SendCoreAsync(
            byte[] data,
            CancellationToken waitCancellationToken,
            CancellationToken ioCancellationToken)
        {
            await _sendLock.WaitAsync(waitCancellationToken).ConfigureAwait(false);
            try
            {
                await SendDataAsync(_client, data, ioCancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public Task<string> Receive() =>
            Receive(CancellationToken.None);

        public Task<string> Receive(CancellationToken cancellationToken) =>
            ReturnWithCallerCancellation(TrackOperation(async () =>
            {
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken))
                {
#if !NET
                    using (linkedCts.Token.Register(CloseSocket))
#endif
                    {
                        byte[] bytes = await ReceiveCoreAsync(
                            linkedCts.Token, _cts.Token).ConfigureAwait(false);
                        return Encoding.UTF8.GetString(bytes);
                    }
                }
            }), cancellationToken);

        private async Task<byte[]> ReceiveCoreAsync(
            CancellationToken waitCancellationToken,
            CancellationToken ioCancellationToken)
        {
            await _receiveLock.WaitAsync(waitCancellationToken).ConfigureAwait(false);
            try
            {
                return await ReceiveAsync(_client, ioCancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _receiveLock.Release();
            }
        }

        public void StopReceiving()
        {
            lock (_lifecycleLock)
            {
                if (_stopping)
                    return;
                _stopping = true;
                _cts.Cancel();
            }
            CloseSocket();
        }

        public async Task StopAsync()
        {
            StopReceiving();

            Task[] operations;
            lock (_lifecycleLock)
                operations = _operations.Values.ToArray();

            if (operations.Length == 0)
                return;

            try { await Task.WhenAll(operations).ConfigureAwait(false); }
            catch (OperationCanceledException) { }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
        }

        private Task TrackOperation(Func<Task> operation)
        {
            lock (_lifecycleLock)
            {
                ThrowIfStopping();
                Task task = operation();
                Track(task);
                return task;
            }
        }

        private Task<T> TrackOperation<T>(Func<Task<T>> operation)
        {
            lock (_lifecycleLock)
            {
                ThrowIfStopping();
                Task<T> task = operation();
                Track(task);
                return task;
            }
        }

        private void Track(Task task)
        {
            int id = Interlocked.Increment(ref _nextOperationId);
            _operations.TryAdd(id, task);
            _ = task.ContinueWith(
                completedTask => CompleteOperation(id, completedTask),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        private void CompleteOperation(int id, Task completedTask)
        {
            _operations.TryRemove(id, out Task ignored);
            if (completedTask.IsFaulted)
                _ = completedTask.Exception;
        }

        private static async Task ReturnWithCallerCancellation(
            Task operation,
            CancellationToken cancellationToken)
        {
#if NET
            if (!cancellationToken.CanBeCanceled || operation.IsCompleted)
            {
                await operation.ConfigureAwait(false);
                return;
            }

            var cancellation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (cancellationToken.Register(() => cancellation.TrySetResult(true)))
            {
                await Task.WhenAny(operation, cancellation.Task).ConfigureAwait(false);
                if (operation.IsCompleted)
                {
                    await operation.ConfigureAwait(false);
                    return;
                }
            }
            throw new OperationCanceledException(cancellationToken);
#else
            await operation.ConfigureAwait(false);
#endif
        }

        private static async Task<T> ReturnWithCallerCancellation<T>(
            Task<T> operation,
            CancellationToken cancellationToken)
        {
#if NET
            if (!cancellationToken.CanBeCanceled || operation.IsCompleted)
                return await operation.ConfigureAwait(false);

            var cancellation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (cancellationToken.Register(() => cancellation.TrySetResult(true)))
            {
                await Task.WhenAny(operation, cancellation.Task).ConfigureAwait(false);
                if (operation.IsCompleted)
                    return await operation.ConfigureAwait(false);
            }
            throw new OperationCanceledException(cancellationToken);
#else
            return await operation.ConfigureAwait(false);
#endif
        }

        private void ThrowIfStopping()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SocketClient));
            if (_stopping)
                throw new InvalidOperationException("The socket client is stopping.");
        }

        private void CloseSocket()
        {
            try { _client.Shutdown(SocketShutdown.Both); } catch { }
            try { _client.Disconnect(true); } catch { }
            try { _client.Close(); } catch { }
            try { _client.Dispose(); } catch { }
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
            _cts.Dispose();
            _requestLock.Dispose();
            _sendLock.Dispose();
            _receiveLock.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
