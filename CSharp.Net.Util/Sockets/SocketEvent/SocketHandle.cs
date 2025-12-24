using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public abstract class SocketHandle
    {
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal async Task ReceiveAsync(Socket socket, byte[] buffer)
        {
            if (socket == null)
                throw new Exception("Socket is not connnect.");

            var totalReceived = 0;
            while (totalReceived < buffer.Length)
            {
                var received = await Task.Factory.FromAsync(
                    socket.BeginReceive(buffer, totalReceived, buffer.Length - totalReceived,
                                       SocketFlags.None, null, null), socket.EndReceive);

                if (received == 0)
                    throw new Exception("连接已关闭");

                totalReceived += received;
            }
        }

        /// <summary>
        /// 异步接收数据
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        protected virtual async Task<byte[]> ReceiveAsync(Socket socket)
        {
            if (socket == null)
                throw new Exception("Socket is not connnect.");

            // 1. 收 header
            byte[] header = await ReceiveExactAsync(socket, 4);
            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header, 0));

            // 2. 收 body
            return await ReceiveExactAsync(socket, length);

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
        private static async Task<byte[]> ReceiveExactAsync(Socket socket, int length)
        {
            byte[] buffer = new byte[length];
            int offset = 0;

            while (offset < length)
            {
                var received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, length - offset), SocketFlags.None);
                if (received == 0) throw new SocketException((int)SocketError.ConnectionReset);
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
        protected virtual async Task SendDataAsync(Socket socket, byte[] data)
        {
            if (socket == null)
                throw new Exception("Socket is not connnect.");
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
                var sent = await socket.SendAsync(new ArraySegment<byte>(packet, offset, packet.Length - offset), SocketFlags.None);
                if (sent == 0) throw new SocketException((int)SocketError.ConnectionReset);
                offset += sent;
            }
            return;
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
            if (socket == null)
                throw new Exception("Socket is not connnect.");

            int totalSent = 0;
            while (totalSent < bytes.Length)
            {
                int sent = socket.Send(bytes, totalSent, bytes.Length - totalSent, SocketFlags.None);
                if (sent == 0) throw new Exception("发送失败，连接可能已断开");
                totalSent += sent;
            }
            //var count = socket.Send(bytes, SocketFlags.None);
            Console.WriteLine($"{DateTime.Now.ToString(1)} 发送了{totalSent}个字节，应发送{bytes.Length}个");
        }

        /// <summary>
        /// 异步接收文件（支持断点续传）
        /// </summary>
        public static async Task ReceiveFileAsync(Socket socket, string savePath, long existingLength = 0)
        {
            using (var fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                if (existingLength > 0) fs.Seek(existingLength, SeekOrigin.Begin);

                while (true)
                {
                    // 收 header
                    byte[] header = await ReceiveExactAsync(socket, 12);
                    int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header, 0));
                    long offset = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(header, 4));

                    // 收正文
                    byte[] chunk = await ReceiveExactAsync(socket, length);

                    // 定位到偏移位置写入
                    fs.Seek(offset, SeekOrigin.Begin);
                    await fs.WriteAsync(chunk, 0, length);

                    // 如果对方传输完毕（主动关闭连接），跳出
                    if (length == 0) break;
                }
            }
        }

    }
}
