using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// Socket消息数据内容
    /// </summary>
    public class MsgContent
    {
        /// <summary>
        /// 回调ID(GUID)
        /// </summary>
        public string CallbackId { get; set; }

        /// <summary>
        /// 消息数据
        /// </summary>
        public string Content { get; set; }

        public MsgContent()
        {
            CallbackId = Guid.NewGuid().ToString("N");
        }
    }

    /// <summary>
    /// 用于回调的Socket封装
    /// </summary>
    public class CallbackSocket
    {
        protected Socket _socket;
        protected ClientSocket _clientSocket;

        public CallbackSocket(Socket socket)
        {
            _socket = socket;
        }

        public CallbackSocket(ClientSocket clientSocket)
        {
            _clientSocket = clientSocket;
        }

        public void SendResult(SockerHelper socketServer, SocketResult socketResult, SocketReceivedEventArgs e)
        {
            SocketData data = new SocketData();
            data.Type = 6;
            data.SocketResult = socketResult;
            socketResult.CallbackId = e.Content.CallbackId;
            socketServer.Send(_clientSocket, data);
        }
    }
    public class SocketData
    {
        /// <summary>
        /// Socket包头
        /// </summary>
        public static readonly string HeaderString = "0XFF";

        /// <summary>
        /// Socket包头
        /// </summary>
        public static readonly byte[] HeaderBytes = Encoding.ASCII.GetBytes(SocketData.HeaderString);

        /// <summary>
        /// 类型 1心跳 2心跳应答 3注册包 4注册反馈 5消息数据 6返回值
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 消息数据
        /// </summary>
        public MsgContent Content { get; set; }

        /// <summary>
        /// 操作结果
        /// </summary>
        public SocketResult SocketResult { get; set; }

        /// <summary>
        /// 注册包数据
        /// </summary>
        public SocketRegisterData SocketRegisterData { get; set; }
    }
    /// <summary>
    /// Socket返回
    /// </summary> 
    public class SocketResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 回调ID
        /// </summary>
        public string CallbackId { get; set; }

        /// <summary>
        /// 收到返回值时间
        /// </summary>
        public DateTime CallbackTime { get; set; }
    }
    /// <summary>
    /// 客户端注册包数据
    /// </summary>
    public class SocketRegisterData
    {
        /// <summary>
        /// Socket客户端ID
        /// </summary>
        public string SocketClientId { get; set; }
    }

    /// <summary>
    /// 客户端Socket对象
    /// </summary>
    public class ClientSocket
    {
        /// <summary>
        /// Socket客户端ID
        /// </summary>
        public string SocketClientId { get; set; }

        /// <summary>
        /// Socket对象
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// 异步参数
        /// </summary>
        public SocketAsyncEventArgs SocketAsyncArgs { get; set; }

        /// <summary>
        /// 异步接收方法
        /// </summary>
        public EventHandler<SocketAsyncEventArgs> SocketAsyncCompleted { get; set; }

        /// <summary>
        /// 客户端操作结果回调
        /// </summary>
        public ConcurrentDictionary<string, SocketResult> CallbackDict = new ConcurrentDictionary<string, SocketResult>();

        /// <summary>
        /// 上次心跳时间
        /// </summary>
        public DateTime LastHeartbeat { get; set; }

        /// <summary>
        /// 缓冲区
        /// </summary>
        public List<byte> Buffer { get; set; }

        /// <summary>
        /// 锁
        /// </summary>
        public object LockSend { get; set; }

        /// <summary>
        /// 客户端Socket对象
        /// </summary>
        public ClientSocket(Socket socket)
        {
            Socket = socket;
            Buffer = new List<byte>();
            LastHeartbeat = DateTime.Now;
            LockSend = new object();
        }

        /// <summary>
        /// 删除接收到的一个包
        /// </summary>
        public void RemoveBufferData(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (Buffer.Count > 0)
                {
                    Buffer.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// 解析字节数组
        /// </summary>
        public SocketData ResolveBuffer(out int readLength)
        {
            SocketData socketData = null;
            readLength = 0;
            var buffer = this.Buffer;
            try
            {
                if (buffer.Count < 4) return null;
                byte[] bArrHeader = new byte[4];
                buffer.CopyTo(bArrHeader, 0, 0, bArrHeader.Length);
                readLength += bArrHeader.Length;
                string strHeader = Encoding.ASCII.GetString(bArrHeader);
                if (strHeader.ToUpper() == SocketData.HeaderString)
                {
                    if (buffer.Count < 5) return null;
                    byte[] bArrType = new byte[1];
                    buffer.CopyTo(bArrType, 4, 0, bArrType.Length);
                    readLength += bArrType.Length;
                    byte bType = bArrType[0];
                    socketData = new SocketData();
                    socketData.Type = bType;

                    string jsonString = null;
                    if (socketData.Type == 3 || socketData.Type == 5 || socketData.Type == 6)
                    {
                        if (buffer.Count < 9) return null;
                        byte[] bArrLength = new byte[4];
                        buffer.CopyTo(bArrLength, 5, 0, bArrLength.Length);
                        readLength += bArrLength.Length;
                        int dataLength = BitConverter.ToInt32(bArrLength, 0);

                        if (dataLength == 0 || buffer.Count < dataLength + 9) return null;
                        byte[] dataBody = new byte[dataLength];
                        buffer.CopyTo(dataBody, 9, 0, dataBody.Length);
                        readLength += dataBody.Length;
                        jsonString = Encoding.UTF8.GetString(dataBody);
                    }

                    if (socketData.Type == 3)
                    {
                        socketData.SocketRegisterData = JsonHelper.Deserialize<SocketRegisterData>(jsonString);
                    }

                    if (socketData.Type == 6)
                    {
                        socketData.SocketResult = JsonHelper.Deserialize<SocketResult>(jsonString);
                    }

                    if (socketData.Type == 5)
                    {
                        socketData.Content = JsonHelper.Deserialize<MsgContent>(jsonString);
                    }
                }
                else
                {
                    LogHelper.Error("不是" + SocketData.HeaderString);
                    return null;
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "解析字节数组 出错");
                return null;
            }

            return socketData;
        }
    }
}
