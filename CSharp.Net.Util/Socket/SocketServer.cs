using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public class SocketServer : IDisposable
    {
        IPAddress _ipAddress = null;
        int _port = 80;
        IPEndPoint ipEndPoint = null;
        Socket _listener = null;

        public SocketServer(int port)
        {
            _port = port;
            ipEndPoint = new IPEndPoint(IPAddress.Any, _port);
            listen();
        }

        public SocketServer(string hostOrAddress, int port = 80)
        {
            _port = port;
            IPHostEntry ipHostInfo = Dns.GetHostEntry(hostOrAddress);
            _ipAddress = ipHostInfo.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            listen();
        }
        //public Socket ListenHost(string hostOrAddress, int port = 80)
        //{
        //    if (_ipAddress != null)
        //        throw new Exception("Server client is created");
        //    IPHostEntry ipHostInfo = Dns.GetHostEntry(hostOrAddress);
        //    _ipAddress =  ipHostInfo.AddressList[0];
        //    _port = port > 0 ? port : _port;
        //    return listen();
        //}
        //public Socket ListenIP(string ip, int port = 80)
        //{
        //    if (_ipAddress != null)
        //        throw new Exception("Server client is created");
        //    _ipAddress = IPAddress.Parse(ip);
        //    _port = port > 0 ? port : _port;
        //    return listen();
        //}

        Socket listen()
        {
            if (_listener != null)
                return _listener;
            if (ipEndPoint == null)
                ipEndPoint = new IPEndPoint(_ipAddress, _port);
            _listener = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(ipEndPoint);
            _listener.Listen(5000);
            Trace.WriteLine("Server is start.");
            return _listener;
        }

        public async Task<string> Receive()
        {
            if (_listener == null) throw new Exception("Listener is not created");
            Socket handler = await _listener.AcceptAsync();
            var epoint = (handler.RemoteEndPoint as IPEndPoint);
            //Trace.WriteLine(epoint.Address + ":" + epoint.Port + "已连接");
            string formAddress = epoint.Address.ToString();
            int port = epoint.Port;

            var data = await socketReceiveData(handler);
            //var eom = "<|EOM|>";
            //if (response.IndexOf(eom) > -1 /* is end of message */)
            //{
            //Trace.WriteLine($"received {handler.RemoteEndPoint.ToString()} message: \"{response.Replace(eom, "")}\"");
            //var ackMessage = "<|ACK|>";
            //var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
            //await handler.SendAsync(echoBytes, 0);
            //Trace.WriteLine($"Socket server sent acknowledgment: \"{ackMessage}\"");
            //break;
            //}
            socketClose(handler);
            return data;
        }

        public async Task<string> ReceiveAndSend(string sendData)
        {
            if (_listener == null) throw new Exception("Listener is not created");
            SocketHandler handler = new SocketHandler(await _listener.AcceptAsync());
            //var epoint = (handler.RemoteEndPoint as IPEndPoint);
            //string formAddress = epoint.Address.ToString();
            //int port = epoint.Port;
            var data = await handler.Receive();// await socketReceiveData(handler);

            //var sendBytes = Encoding.UTF8.GetBytes(sendData);
#if NET
            handler.Send(sendData);
#else
              handler.Send(sendData);
#endif
            //handler.Dispose();
            // socketClose(handler);
            return data;
        }


        public async void Send(string sendData)
        {
            if (_listener == null) throw new Exception("Listener is not created");
            SocketHandler handler = new SocketHandler(await _listener.AcceptAsync());

            handler.Send(sendData);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        async Task<string> socketReceiveData(Socket socket)
        {
            if (socket == null) return null;
            var buffer = new byte[1_024];
            string data = ""; int received = 0;
#if NET
            do
            {
                received = await socket.ReceiveAsync(buffer, SocketFlags.None);
                data += Encoding.UTF8.GetString(buffer, 0, received);
            } while (received == buffer.Length);
#else
            received = socket.Receive(buffer, SocketFlags.None);
            data = Encoding.UTF8.GetString(buffer, 0, received);
#endif
            return data;
        }

        /// <summary>
        /// 释放会话
        /// </summary>
        /// <param name="socket"></param>
        async void socketClose(Socket socket)
        {
            if (socket == null) return;
            socket.Shutdown(SocketShutdown.Both);
#if NET
            await socket.DisconnectAsync(true);
#else
            socket.Disconnect(true);
#endif
            socket.Close();
            socket.Dispose();
        }

        public void Dispose()
        {
            if (_listener != null)
            {
                _listener.Shutdown(SocketShutdown.Both);
                _listener.Disconnect(true);
                _listener.Close();
                _listener.Dispose();
            }
            if (ipEndPoint != null)
                ipEndPoint = null;
            if (_ipAddress != null)
                _ipAddress = null;
        }
    }
}
