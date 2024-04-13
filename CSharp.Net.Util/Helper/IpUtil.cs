using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// IpHelper
    /// 获取客户端ip地址请使用ZJHW_Common_System.Mvc.General.IPAddress
    /// </summary>
    public sealed class IpUtil
    {
        /// <summary>
        /// 校验IP地址
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>返回是否符合Ip地址格式</returns>
        public static bool CheckIp(string ip)
        {
            var regex = new Regex(@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
            return regex.IsMatch(ip);
        }

        /// <summary>
        /// ip地址转换为长整型
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static long ToLongIP(string ip)
        {
            if (!CheckIp(ip))
                throw new ArgumentException("校验IP地址错误");
            string[] arr = ip.Split('.');

            long ip1 = Int64.Parse(arr[0]) * 256 * 256 * 256;
            long ip2 = Int64.Parse(arr[1]) * 256 * 256;
            long ip3 = Int64.Parse(arr[2]) * 256;
            long ip4 = Int64.Parse(arr[3]);

            return ip1 + ip2 + ip3 + ip4;
        }

        /// <summary>
        /// ip转换8进制
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string To8x(string ip)
        {
            var n = ToLongIP(ip);

            return "00" + Convert.ToString(n, 8);
        }

        /// <summary>
        /// ip转换16进制
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string To16x(string ip)
        {
            var n = ToLongIP(ip);

            return "0x" + Convert.ToString(n, 16);
        }

        /// <summary>
        /// 获取当前服务器IP
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            var ip = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().Select(p => p.GetIPProperties()).SelectMany(p => p.UnicastAddresses)
                  .Where(p => p.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(p.Address))
                  .FirstOrDefault()?.Address.MapToIPv4()?.ToString();
            return ip;
        }

        private void ConnectWithTimeout(Socket socket, EndPoint endpoint, int timeout, EventHandler<SocketAsyncEventArgs> OnConnectCompleted)
        {
            if (endpoint is DnsEndPoint && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IPAddress[] addresses;
                var dnsEndPoint = ((DnsEndPoint)endpoint);
                var host = dnsEndPoint.Host;
                var method = typeof(System.Net.Dns).GetTypeInfo().GetMethod("InternalGetHostByName", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    addresses = ((IPHostEntry)method.Invoke(null, new object[] { host, false })).AddressList;
                }
                else
                {
                    Task<IPAddress[]> task = Dns.GetHostAddressesAsync(host);
                    task.Wait(timeout);
                    addresses = task.Result;
                }

                var address = addresses.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                if (address == null)
                {
                    throw new ArgumentException(String.Format("Could not resolve host '{0}'.", host));
                }
                endpoint = new IPEndPoint(address, dnsEndPoint.Port);
            }

            var completed = new AutoResetEvent(false);
            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = endpoint;
            args.Completed += OnConnectCompleted;
            args.UserToken = completed;
            socket.ConnectAsync(args);
            if (!completed.WaitOne(timeout) || !socket.Connected)
            {
                using (socket)
                {
                    throw new TimeoutException("Could not connect to " + endpoint);
                }
            }
        }

    }
}
