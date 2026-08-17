using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public abstract class SocketHandle
    {
        public const int DefaultMaxPacketSize = 16 * 1024 * 1024;

        protected SocketHandle()
            : this(DefaultMaxPacketSize)
        {
        }

        protected SocketHandle(int maxPacketSize)
        {
            if (maxPacketSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxPacketSize));
            MaxPacketSize = maxPacketSize;
        }

        public int MaxPacketSize { get; }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal Task ReceiveAsync(Socket socket, byte[] buffer) =>
            ReceiveAsync(socket, buffer, CancellationToken.None);

        internal async Task ReceiveAsync(Socket socket, byte[] buffer, CancellationToken cancellationToken)
        {
            EnsureConnected(socket);
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            var totalReceived = 0;
            while (totalReceived < buffer.Length)
            {
                int received = await ReceiveChunkAsync(
                    socket, buffer, totalReceived, buffer.Length - totalReceived, cancellationToken).ConfigureAwait(false);

                if (received == 0)
                    throw new SocketException((int)SocketError.ConnectionReset);

                totalReceived += received;
            }
        }

        /// <summary>
        /// 异步接收数据
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        protected virtual Task<byte[]> ReceiveAsync(Socket socket) =>
            ReceiveAsync(socket, CancellationToken.None);

        protected virtual async Task<byte[]> ReceiveAsync(Socket socket, CancellationToken cancellationToken)
        {
            EnsureConnected(socket);

            // 1. 收 header
            byte[] header = await ReceiveExactAsync(socket, 4, cancellationToken).ConfigureAwait(false);
            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header, 0));
            ValidatePacketLength(length, MaxPacketSize);

            // 2. 收 body
            return await ReceiveExactAsync(socket, length, cancellationToken).ConfigureAwait(false);

            /*
            var buffer = new byte[1_024 * 1024]; //每次接收1MB的数据
            int received = 0;
            using (var ms = new MemoryStream())
            {
                do
                {
                    received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (received > 0)
                    {
                        ms.Write(buffer, 0, received);
                    }
                } while (received == buffer.Length); // 当接收到的数据小于缓冲区大小时，说明消息接收完毕

                return ms.ToArray();
            }*/
        }

        /// <summary>
        /// 精确接收指定长度的数据
        /// </summary>
        private static async Task<byte[]> ReceiveExactAsync(
            Socket socket,
            int length,
            CancellationToken cancellationToken)
        {
            EnsureConnected(socket);
            if (length < 0)
                throw new InvalidDataException("Packet length cannot be negative.");

            byte[] buffer = new byte[length];
            int offset = 0;

            while (offset < length)
            {
                int received = await ReceiveChunkAsync(
                    socket, buffer, offset, length - offset, cancellationToken).ConfigureAwait(false);
                if (received == 0)
                    throw new SocketException((int)SocketError.ConnectionReset);
                offset += received;
            }
            return buffer;
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected virtual Task SendDataAsync(Socket socket, byte[] data) =>
            SendDataAsync(socket, data, CancellationToken.None);

        protected virtual async Task SendDataAsync(Socket socket, byte[] data, CancellationToken cancellationToken)
        {
            EnsureConnected(socket);
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            ValidatePacketLength(data.Length, MaxPacketSize);

            /*
            var header = BitConverter.GetBytes(data.Length);
            // 确保使用大端字节序
            if (BitConverter.IsLittleEndian)
                Array.Reverse(header);
            */

            byte[] header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
            byte[] packet = new byte[4 + data.Length];//header.Length

            Buffer.BlockCopy(header, 0, packet, 0, 4);
            Buffer.BlockCopy(data, 0, packet, 4, data.Length);

            int offset = 0;
            while (offset < packet.Length)
            {
                int sent = await SendChunkAsync(
                    socket, packet, offset, packet.Length - offset, cancellationToken).ConfigureAwait(false);
                if (sent == 0)
                    throw new SocketException((int)SocketError.ConnectionReset);
                offset += sent;
            }

            /*
            var totalSent = 0;
            while (totalSent < data.Length)
            {
                var sent = await Task.Factory.FromAsync(
                    socket.BeginSend(buffer, totalSent, buffer.Length - totalSent,
                                    SocketFlags.None, null, null),
                    socket.EndSend);

                if (sent == 0)
                    throw new Exception("发送数据失败");

                totalSent += sent;
            }*/
        }

        /// <summary>
        /// 同步发送消息
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="bytes"></param>
        /// <exception cref="Exception"></exception>
        public void SendBytes(Socket socket, byte[] bytes)
        {
            EnsureConnected(socket);
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            int totalSent = 0;
            while (totalSent < bytes.Length)
            {
                int sent = socket.Send(bytes, totalSent, bytes.Length - totalSent, SocketFlags.None);
                if (sent == 0)
                    throw new SocketException((int)SocketError.ConnectionReset);
                totalSent += sent;
            }
            //var count = socket.Send(bytes, SocketFlags.None);
            Console.WriteLine($"{DateTime.Now.ToString(1)} 发送了{totalSent}个字节，应发送{bytes.Length}个");
        }

        /// <summary>
        /// 异步接收文件（支持断点续传）
        /// </summary>
        public static Task ReceiveFileAsync(Socket socket, string savePath, long existingLength = 0) =>
            ReceiveFileAsync(socket, savePath, existingLength, DefaultMaxPacketSize, CancellationToken.None);

        public static async Task ReceiveFileAsync(
            Socket socket,
            string savePath,
            long existingLength,
            int maxPacketSize,
            CancellationToken cancellationToken = default)
        {
            if (existingLength < 0)
                throw new ArgumentOutOfRangeException(nameof(existingLength));
            if (maxPacketSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxPacketSize));

            using (var fs = new FileStream(
                savePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous))
            {
                if (existingLength > 0) fs.Seek(existingLength, SeekOrigin.Begin);

                while (true)
                {
                    // 收 header
                    byte[] header = await ReceiveExactAsync(socket, 12, cancellationToken).ConfigureAwait(false);
                    int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header, 0));
                    ValidatePacketLength(length, maxPacketSize);
                    long offset = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(header, 4));
                    if (offset < 0)
                        throw new InvalidDataException("File chunk offset cannot be negative.");

                    // 如果对方传输完毕（主动关闭连接），跳出
                    if (length == 0) break;

                    // 收正文
                    byte[] chunk = await ReceiveExactAsync(socket, length, cancellationToken).ConfigureAwait(false);

                    // 定位到偏移位置写入
                    fs.Seek(offset, SeekOrigin.Begin);
                    await fs.WriteAsync(chunk, 0, length, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static void ValidatePacketLength(int length, int maxPacketSize)
        {
            if (length < 0 || length > maxPacketSize)
                throw new InvalidDataException($"Packet length {length} is outside the allowed range 0..{maxPacketSize}.");
        }

        private static void EnsureConnected(Socket socket)
        {
            if (socket == null)
                throw new InvalidOperationException("Socket is not connected.");
        }

        private static async Task<int> ReceiveChunkAsync(
            Socket socket,
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
#if NET
            return await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer, offset, count), SocketFlags.None, cancellationToken).ConfigureAwait(false);
#else
            cancellationToken.ThrowIfCancellationRequested();
            return await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer, offset, count), SocketFlags.None).ConfigureAwait(false);
#endif
        }

        private static async Task<int> SendChunkAsync(
            Socket socket,
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
#if NET
            return await socket.SendAsync(
                new ArraySegment<byte>(buffer, offset, count), SocketFlags.None, cancellationToken).ConfigureAwait(false);
#else
            cancellationToken.ThrowIfCancellationRequested();
            return await socket.SendAsync(
                new ArraySegment<byte>(buffer, offset, count), SocketFlags.None).ConfigureAwait(false);
#endif
        }
    }
}
