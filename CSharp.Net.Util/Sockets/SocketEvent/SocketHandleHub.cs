using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public abstract class SocketHandleHub : SocketHandle
    {
        protected CancellationTokenSource? _cts;

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        protected override async Task<byte[]> ReceiveAsync(Socket socket)
        {
            if (socket == null) return null;

            /*
            // 先读取4字节的消息头
            var headerBuffer = new byte[4];
            int received = await socket.ReceiveAsync(new ArraySegment<byte>(headerBuffer), SocketFlags.None);
            string header = Encoding.UTF8.GetString(headerBuffer);
            if (received < 4 || header != "MSG_")
                throw new SocketPacketException("数据头错误," + received);
            */

            // 读取长度
            var lengthBytes = new byte[4];
            await base.ReceiveAsync(socket, lengthBytes);
            //received = await socket.ReceiveAsync(new ArraySegment<byte>(lengthBytes), SocketFlags.None);
            //if (received < 4) throw new SocketPacketException("数据长度接收错误," + received);

            {
                // 确保使用大端字节序
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lengthBytes);

                int dataLength = BitConverter.ToInt32(lengthBytes, 0);
                byte[] buffer = new byte[dataLength];

                await base.ReceiveAsync(socket, buffer);
                return buffer;
            }
            {
                /*
                int totalReceived = 0;
                while (totalReceived < dataLength)
                {
                    int toReceive = Math.Min(buffer.Length, dataLength - totalReceived);
                    received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer, totalReceived, toReceive), SocketFlags.None);
                    if (received == 0)
                        throw new SocketPacketException("连接已断开，数据未接收完整");

                    totalReceived += received;
                }
                return buffer;
                */
            }
        }

        /// <summary>
        /// 接收协议包数据：消息头+长度+数据
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected async Task SendDataAsync(Socket socket, string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await SendDataAsync(socket, messageBytes);
        }

        /// <summary>
        /// 接收协议包数据：消息头+长度+数据
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected override async Task SendDataAsync(Socket socket, byte[] bytes)
        {
            //合并发送
            await base.SendDataAsync(socket, bytes);

            /*
            // 发送长度前缀
            await base.SendDataAsync(socket, lengthBytes);
            // 发送消息内容
            await base.SendDataAsync(socket, bytes);
            */

            //byte[] header = Encoding.UTF8.GetBytes("MS_"); // 4字节的消息头
            //byte[] lengthBytes = BitConverter.GetBytes(data.Length); // 4字节的数据长度

            ////发送 头部 + 长度 + 数据
            //await client.SendAsync(new ArraySegment<byte>(header), SocketFlags.None);
            //await client.SendAsync(new ArraySegment<byte>(lengthBytes), SocketFlags.None);
            //await client.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
        }
    }
}
