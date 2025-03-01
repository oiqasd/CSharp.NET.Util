﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Linq;
using System.Diagnostics;

namespace CSharp.Net.Util
{
    public class SockerHelper
    {
        #region 变量
        private int _serverPort;
        private Socket serverSocket;
        private ConcurrentDictionary<ClientSocket, string> clientSocketList = new ConcurrentDictionary<ClientSocket, string>();
        private ConcurrentDictionary<string, ClientSocket> _dictClientIdClientSocket = new ConcurrentDictionary<string, ClientSocket>();

        public int _CallbackTimeout = 20;
        /// <summary>
        /// 等待回调超时时间(单位：秒)
        /// </summary>
        public int CallbackTimeout
        {
            get { return _CallbackTimeout; }
            set { value = _CallbackTimeout; }
        }

        public int _WaitResultTimeout = 20;
        /// <summary>
        /// 等待返回结果超时时间(单位：秒)
        /// </summary>
        public int WaitResultTimeout
        {
            get { return _WaitResultTimeout; }
            set { value = _WaitResultTimeout; }
        }

        private object _lockSend = new object();

        /// <summary>
        /// 接收反馈消息事件
        /// </summary>
        public event EventHandler<ReceivedSocketResultEventArgs> ReceivedSocketResultEvent;

        /// <summary>
        /// Socket客户端离线事件
        /// </summary>
        public event EventHandler<SocketClientOfflineEventArgs> SocketClientOfflineEvent;

        /// <summary>
        /// Socket客户端注册事件
        /// </summary>
        public event EventHandler<SocketClientRegisterEventArgs> SocketClientRegisterEvent;

        /// <summary>
        /// 接收消息事件
        /// </summary>
        public event EventHandler<SocketReceivedEventArgs> SocketReceivedEvent;

        private System.Timers.Timer _checkClientTimer;

        /// <summary>
        /// 清理数据Timer
        /// </summary>
        private System.Timers.Timer _clearDataTimer;

        #endregion

        #region 构造函数
        public SockerHelper(int serverPort)
        {
            _serverPort = serverPort;

            _clearDataTimer = new System.Timers.Timer();
            _clearDataTimer.Interval = 60 * 1000;
            _clearDataTimer.Elapsed += _clearDataTimer_Elapsed;
            _clearDataTimer.Start();
        }
        #endregion

        #region 启动服务
        /// <summary>
        /// 启动服务
        /// </summary>
        public bool StartServer()
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, _serverPort);
                serverSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(5000);
                Thread thread = new Thread(new ThreadStart(delegate ()
                {
                    while (true)
                    {
                        Socket client = null;
                        ClientSocket clientSocket = null;

                        try
                        {
                            client = serverSocket.Accept();
                            client.SendTimeout = 20000;
                            client.ReceiveTimeout = 20000;
                            client.SendBufferSize = 10240;
                            client.ReceiveBufferSize = 10240;
                            clientSocket = new ClientSocket(client);
                            clientSocketList.TryAdd(clientSocket, null);
                            Trace.WriteLine("监听到新的客户端，当前客户端数：" + clientSocketList.Count);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                            Thread.Sleep(1);
                            continue;
                        }

                        if (client == null) continue;

                        try
                        {
                            byte[] buffer = new byte[10240];
                            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                            clientSocket.SocketAsyncArgs = args;
                            clientSocket.SocketAsyncCompleted = (s, e) =>
                            {
                                ReceiveData(clientSocket, e);
                            };
                            args.SetBuffer(buffer, 0, buffer.Length);
                            args.Completed += clientSocket.SocketAsyncCompleted;
                            client.ReceiveAsync(args);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }
                    }
                }));
                thread.IsBackground = true;
                thread.Start();

                //检测客户端
                _checkClientTimer = new System.Timers.Timer();
                _checkClientTimer.AutoReset = false;
                _checkClientTimer.Interval = 1000;
                _checkClientTimer.Elapsed += CheckClient;
                _checkClientTimer.Start();

                LogHelper.Info("服务已启动");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "启动服务出错");
                return false;
            }
        }
        #endregion

        #region 检测客户端
        /// <summary>
        /// 检测客户端
        /// </summary>
        private void CheckClient(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;

                foreach (ClientSocket clientSkt in clientSocketList.Keys.ToArray())
                {
                    Socket skt = clientSkt.Socket;

                    if (now.Subtract(clientSkt.LastHeartbeat).TotalSeconds > 15)
                    {
                        ReleaseClientSocket(clientSkt, skt);
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "检测客户端出错");
            }
            finally
            {
                _checkClientTimer.Start();
            }
        }

        /// <summary>
        /// 释放客户端
        /// </summary>
        private void ReleaseClientSocket(ClientSocket clientSkt, Socket skt)
        {
            string strTemp;
            ClientSocket clientSocketTemp;
            _dictClientIdClientSocket.TryRemove(clientSkt.SocketClientId, out clientSocketTemp);
            clientSocketList.TryRemove(clientSkt, out strTemp);
            if (clientSkt.SocketClientId != null && SocketClientOfflineEvent != null)
            {
                SocketClientOfflineEventArgs socketClientOfflineEventArgs = new SocketClientOfflineEventArgs(clientSkt.SocketClientId);
                ThreadUtil.Run(() =>
                {
                    SocketClientOfflineEvent(null, socketClientOfflineEventArgs);
                });
            }
            LogHelper.Info("客户端已失去连接，当前客户端数：" + clientSocketList.Count);
            Try.Action(() => { if (skt.Connected) skt.Disconnect(false); });
            Try.Action(() =>
            {
                skt.Close();
                skt.Dispose();
                if (clientSkt.SocketAsyncArgs != null)
                {
                    if (clientSkt.SocketAsyncCompleted != null)
                    {
                        clientSkt.SocketAsyncArgs.Completed -= clientSkt.SocketAsyncCompleted;
                    }
                    clientSkt.SocketAsyncArgs.Dispose();
                }
                clientSkt.SocketAsyncCompleted = null;
                clientSkt.SocketAsyncArgs = null;
            });
        }
        #endregion

        #region 接收数据
        /// <summary>
        /// 处理接收的数据包
        /// </summary>
        private void ReceiveData(ClientSocket clientSkt, SocketAsyncEventArgs e)
        {
            if (clientSkt == null) return;
            Socket skt = clientSkt.Socket;

            try
            {
                if (e.BytesTransferred == 0)
                {
                    ReleaseClientSocket(clientSkt, skt);
                    return;
                }

                e.Buffer.CopyTo(clientSkt.Buffer, 0, e.BytesTransferred);

                #region 校验数据
                if (clientSkt.Buffer.Count < 4)
                {
                    if (skt.Connected)
                    {
                        if (!skt.ReceiveAsync(e)) ReceiveData(clientSkt, e);
                    }
                    return;
                }
                else
                {
                    byte[] bArrHeader = new byte[4];
                    clientSkt.Buffer.CopyTo(bArrHeader, 0, 0, bArrHeader.Length);
                    string strHeader = Encoding.ASCII.GetString(bArrHeader);
                    if (strHeader.ToUpper() == SocketData.HeaderString)
                    {
                        if (clientSkt.Buffer.Count < 5)
                        {
                            if (skt.Connected)
                            {
                                if (!skt.ReceiveAsync(e)) ReceiveData(clientSkt, e);
                            }
                            return;
                        }
                        else
                        {
                            byte[] bArrType = new byte[1];
                            clientSkt.Buffer.CopyTo(bArrType, 4, 0, bArrType.Length);
                            if (bArrType[0] == (int)1) { } //心跳包
                            else if (bArrType[0] == (int)3 || bArrType[0] == (int)5 || bArrType[0] == (int)6) //注册包、消息数据、返回值包
                            {
                                if (clientSkt.Buffer.Count < 9)
                                {
                                    if (skt.Connected)
                                    {
                                        if (!skt.ReceiveAsync(e)) ReceiveData(clientSkt, e);
                                    }
                                    return;
                                }
                                else
                                {
                                    byte[] bArrLength = new byte[4];
                                    clientSkt.Buffer.CopyTo(bArrLength, 5, 0, bArrLength.Length);
                                    int dataLength = BitConverter.ToInt32(bArrLength, 0);
                                    if (dataLength == 0 || clientSkt.Buffer.Count < dataLength + 9)
                                    {
                                        if (skt.Connected)
                                        {
                                            if (!skt.ReceiveAsync(e)) ReceiveData(clientSkt, e);
                                        }
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                LogHelper.Error(string.Format("type错误，丢掉错误数据，重新接收，SocketClientId={0}", clientSkt.SocketClientId));
                                clientSkt.Buffer.Clear(); //把错误的数据丢掉
                                if (skt.Connected)
                                {
                                    if (!skt.ReceiveAsync(e)) ReceiveData(clientSkt, e);
                                }
                                return;
                            }
                        }
                    }
                    else
                    {
                        LogHelper.Error(string.Format("不是" + SocketData.HeaderString + "，丢掉错误数据，重新接收，SocketClientId={0}", clientSkt.SocketClientId));
                        LogHelper.Error(clientSkt.Buffer.ToStringValue());
                        clientSkt.Buffer.Clear(); //把错误的数据丢掉
                        if (skt.Connected)
                        {
                            if (!skt.ReceiveAsync(e)) ReceiveData(clientSkt, e);
                        }
                        return;
                    }
                }
                #endregion

                SocketData data = null;
                do
                {
                    data = ProcessSocketData(clientSkt);
                } while (data != null);

                if (skt.Connected)
                {
                    if (!skt.ReceiveAsync(e)) ReceiveData(clientSkt, e);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "处理接收的数据包 异常");
            }
        }


        #endregion

        #region 处理接收的数据包
        /// <summary>
        /// 处理接收的数据包
        /// </summary>
        private SocketData ProcessSocketData(ClientSocket clientSkt)
        {
            int readLength = 0;
            SocketData data = clientSkt.ResolveBuffer(out readLength);
            if (data != null)
            {
                if (readLength > 0) clientSkt.RemoveBufferData(readLength);
                if (data.Type == 1) //收到心跳包
                {
                    clientSkt.LastHeartbeat = DateTime.Now;

                    //心跳应答
                    if (clientSkt.SocketClientId != null)
                    {
                        byte[] bArr = new byte[5];
                        byte[] bArrHeader = SocketData.HeaderBytes;
                        bArrHeader.CopyTo(bArr, 0, 0, bArrHeader.Length);
                        bArr[4] = (byte)2;

                        lock (clientSkt.LockSend)
                        {
                            Send(clientSkt.Socket, bArr);
                        }
                    }
                    else
                    {
                        LogHelper.Info("没有注册信息");
                    }

                    //LogUtil.Log("收到心跳包，客户端连接正常，SocketClientId=" + clientSkt.SocketClientId);
                }

                if (data.Type == 3) //收到注册包
                {
                    if (data.SocketRegisterData != null && clientSkt != null)
                    {
                        ClientSocket temp;
                        if (data.SocketRegisterData.SocketClientId != null) _dictClientIdClientSocket.TryRemove(data.SocketRegisterData.SocketClientId, out temp);
                        clientSkt.SocketClientId = data.SocketRegisterData.SocketClientId;
                        if (data.SocketRegisterData.SocketClientId != null) _dictClientIdClientSocket.TryAdd(data.SocketRegisterData.SocketClientId, clientSkt);
                        LogHelper.Info("收到注册包，SocketClientId=" + clientSkt.SocketClientId);

                        //客户端注册事件
                        if (data.SocketRegisterData.SocketClientId != null && SocketClientRegisterEvent != null)
                        {
                            ThreadUtil.Run(() =>
                            {
                                SocketClientRegisterEvent(null, new SocketClientRegisterEventArgs(data.SocketRegisterData.SocketClientId));
                            });
                        }

                        //注册反馈
                        List<byte> byteList = new List<byte>();
                        byte[] bArrHeader = Encoding.ASCII.GetBytes(SocketData.HeaderString);
                        byteList.Append(bArrHeader);
                        byteList.Append(new byte[] { (byte)4 });
                        lock (clientSkt.LockSend)
                        {
                            Send(clientSkt.Socket, byteList.ToArray());
                        }
                    }
                }

                if (data.Type == 6) //收到返回值包
                {
                    if (data.SocketResult != null)
                    {
                        data.SocketResult.CallbackTime = DateTime.Now;
                        clientSkt.CallbackDict.TryAdd(data.SocketResult.CallbackId, data.SocketResult);
                    }

                    if (ReceivedSocketResultEvent != null)
                    {
                        ThreadUtil.Run(() =>
                        {
                            ReceivedSocketResultEvent(null, new ReceivedSocketResultEventArgs(data.SocketResult));
                        });
                    }

                    //LogUtil.Log("收到返回值包，SocketClientId=" + clientSkt.SocketClientId);
                }

                if (data.Type == 5 && SocketReceivedEvent != null) //收到消息数据
                {
                    SocketReceivedEventArgs socketReceivedEventArgs = new SocketReceivedEventArgs(data.Content);
                    socketReceivedEventArgs.Callback = new CallbackSocket(clientSkt);
                    ThreadUtil.Run(() =>
                    {
                        SocketReceivedEvent(null, socketReceivedEventArgs);
                    });
                }
            }
            return data;
        }
        #endregion

        #region 停止服务
        /// <summary>
        /// 停止服务
        /// </summary>
        public void StopServer()
        {
            try
            {
                foreach (ClientSocket clientSocket in clientSocketList.Keys.ToArray())
                {
                    Socket socket = clientSocket.Socket;
                    Try.Action(() => { if (socket.Connected) socket.Disconnect(false); });
                    Try.Action(() =>
                    {
                        socket.Close();
                        socket.Dispose();
                    });
                }
                clientSocketList.Clear();
                _dictClientIdClientSocket.Clear();
                if (serverSocket != null)
                {
                    Try.Action(() => { if (serverSocket.Connected) serverSocket.Disconnect(false); });
                    Try.Action(() =>
                    {
                        serverSocket.Close();
                        serverSocket.Dispose();
                    });
                }
                LogHelper.Info("服务已停止");
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "停止服务出错");
            }
        }
        #endregion

        #region 释放资源
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_checkClientTimer != null)
            {
                _checkClientTimer.Stop();
                _checkClientTimer.Elapsed -= CheckClient;
                _checkClientTimer.Close();
            }
            if (_clearDataTimer != null)
            {
                _clearDataTimer.Stop();
                _clearDataTimer.Elapsed -= _clearDataTimer_Elapsed;
                _clearDataTimer.Close();
            }
        }
        #endregion

        #region Send
        /// <summary>
        /// Send 单个发送 并等待结果
        /// </summary>
        public SocketResult Send(MsgContent content, string socketClientId)
        {
            SocketData data = new SocketData();
            data.Type = 5;
            data.Content = content;

            ClientSocket clientSocket = null;
            if (socketClientId != null) _dictClientIdClientSocket.TryGetValue(socketClientId, out clientSocket);

            if (clientSocket != null)
            {
                Send(clientSocket, data);
                return WaitSocketResult(clientSocket, content.CallbackId);
            }
            else
            {
                SocketResult socketResult = new SocketResult();
                socketResult.Success = false;
                socketResult.Msg = "客户端不存在";
                return socketResult;
            }
        }

        /// <summary>
        /// Send 单个发送
        /// </summary>
        public void Send(MsgContent content, string socketClientId, Action<SocketResult> callback = null)
        {
            SocketData data = new SocketData();
            data.Type = 5;
            data.Content = content;

            ClientSocket clientSocket = null;
            if (socketClientId != null) _dictClientIdClientSocket.TryGetValue(socketClientId, out clientSocket);

            if (clientSocket != null)
            {
                if (callback != null)
                {
                    WaitCallback(clientSocket, content.CallbackId, callback);
                }

                Send(clientSocket, data);
            }
            else
            {
                SocketResult socketResult = new SocketResult();
                socketResult.Success = false;
                socketResult.Msg = "客户端不存在";
                if (callback != null) callback(socketResult);
            }
        }

        /// <summary>
        /// 等待回调
        /// </summary>
        private void WaitCallback(ClientSocket clientSocket, string callbackId, Action<SocketResult> callback = null)
        {
            DateTime dt = DateTime.Now.AddSeconds(_CallbackTimeout);
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = false;
            timer.Interval = 100;
            timer.Elapsed += (s, e) =>
            {
                try
                {
                    SocketResult socketResult;
                    if (!clientSocket.CallbackDict.TryGetValue(callbackId, out socketResult) && DateTime.Now < dt)
                    {
                        timer.Start();
                        return;
                    }
                    SocketResult sktResult;
                    clientSocket.CallbackDict.TryRemove(callbackId, out sktResult);
                    if (socketResult == null)
                    {
                        socketResult = new SocketResult();
                        socketResult.Success = false;
                        socketResult.Msg = "超时";
                    }

                    if (callback != null) callback(socketResult);

                    timer.Close();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("WaitCallback error" + ex);
                }
            };
            timer.Start();
        }

        /// <summary>
        /// 等待SocketResult
        /// </summary>
        private SocketResult WaitSocketResult(ClientSocket clientSocket, string callbackId)
        {
            SocketResult socketResult;
            DateTime dt = DateTime.Now.AddSeconds(_WaitResultTimeout);
            while (!clientSocket.CallbackDict.TryGetValue(callbackId, out socketResult) && DateTime.Now < dt)
            {
                Thread.Sleep(10);
            }
            SocketResult sktResult;
            clientSocket.CallbackDict.TryRemove(callbackId, out sktResult);
            if (socketResult == null)
            {
                socketResult = new SocketResult();
                socketResult.Success = false;
                socketResult.Msg = "超时";
            }
            return socketResult;
        }

        /// <summary>
        /// Send
        /// </summary>
        public void Send(ClientSocket clientSocket, SocketData data)
        {
            Socket socket = clientSocket.Socket;

            List<byte> byteList = new List<byte>();
            byte[] bArrHeader = Encoding.ASCII.GetBytes(SocketData.HeaderString); //header
            byteList.Append(bArrHeader);

            if (data.Type == 2)
            {
                byteList.Append(new byte[] { (byte)2 });//type
            }

            if (data.Type == 5)
            {
                byteList.Append(new byte[] { (byte)5 }); //type

                if (data.Content != null)
                {
                    byte[] bArrData = Encoding.UTF8.GetBytes(JsonHelper.Serialize(data.Content));
                    byteList.Append(BitConverter.GetBytes(bArrData.Length)); //length
                    byteList.Append(bArrData); //body
                }
            }

            if (data.Type == 6)
            {
                byteList.Append(new byte[] { (byte)6 }); //type

                if (data.SocketResult != null)
                {
                    byte[] bArrData = Encoding.UTF8.GetBytes(JsonHelper.Serialize(data.SocketResult));
                    byteList.Append(BitConverter.GetBytes(bArrData.Length)); //length
                    byteList.Append(bArrData); //body
                }
            }

            lock (clientSocket.LockSend)
            {
                Send(socket, byteList.ToArray()); //发送
            }
        }
        #endregion

        #region 获取全部客户端ID集合
        /// <summary>
        /// 获取全部客户端ID集合
        /// </summary>
        public List<string> GetSocketClientIdListAll()
        {
            return _dictClientIdClientSocket.Keys.ToList();
        }
        #endregion

        #region 清理回调数据
        /// <summary>
        /// 清理回调数据
        /// </summary>
        private void _clearDataTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ThreadUtil.Run(() =>
            {
                SocketResult socketResult;
                SocketResult temp;

                foreach (ClientSocket clientSocket in clientSocketList.Keys.ToArray())
                {
                    foreach (string key in clientSocket.CallbackDict.Keys.ToArray())
                    {
                        if (clientSocket.CallbackDict.TryGetValue(key, out socketResult))
                        {
                            if (DateTime.Now.Subtract(socketResult.CallbackTime).TotalSeconds > _CallbackTimeout * 2)
                            {
                                clientSocket.CallbackDict.TryRemove(key, out temp);
                            }
                        }
                    }
                }
            });
        }
        #endregion

        public bool Send(Socket socket, byte[] data)
        {
            try
            {
                if (socket == null || !socket.Connected) return false;

                int sendTotal = 0;
                while (sendTotal < data.Length)
                {
                    int sendLength = data.Length - sendTotal;
                    if (sendLength > 1024) sendLength = 1024;
                    int sendOnce = socket.Send(data, sendTotal, sendLength, SocketFlags.None);
                    sendTotal += sendOnce;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Fatal(ex);
                return false;
            }
        }
    }
}
