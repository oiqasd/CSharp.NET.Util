using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace CSharp.Net.Util.CsHttp
{
    internal sealed class DefaultHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
    {
        private string _name;

        public override string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _name = value;
            }
        }

#if false
        public override HttpMessageHandler PrimaryHandler { get; set; } = new SocketsHttpHandler()
        {
            MaxConnectionsPerServer = 512,
            ConnectTimeout = TimeSpan.FromSeconds(20),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            PooledConnectionLifetime = TimeSpan.FromMinutes(3),
            EnableMultipleHttp2Connections = true,
            ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.NoDelay = true;
                    await socket.ConnectAsync(context.DnsEndPoint, cancellationToken).ConfigureAwait(false);
                    socket.SendTimeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
                    socket.ReceiveTimeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
                }
                catch { socket.Dispose(); throw; }
                return new NetworkStream(socket, ownsSocket: true);//true 自动释放Socket资源
            },
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseCookies = false,
        };
#else
        public override HttpMessageHandler PrimaryHandler { get; set; } = new HttpClientHandler()
        {
            UseDefaultCredentials = true,
            ClientCertificateOptions = ClientCertificateOption.Automatic,
            //关闭证书验证
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true,
            SslProtocols = SslProtocols.None,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            MaxConnectionsPerServer = 512,
            UseCookies = false,
        };
#endif
        public override IList<DelegatingHandler> AdditionalHandlers { get; } = new List<DelegatingHandler>();


        //public override IServiceProvider Services { get; }

        //public DefaultHttpMessageHandlerBuilder(IServiceProvider services)
        //{
        //    Services = services;
        //}

        public override HttpMessageHandler Build()
        {
            if (PrimaryHandler == null)
            {
                string message = "HttpMessageHandlerBuilder_PrimaryHandlerIsNull";
                throw new InvalidOperationException(message);
            }
            return HttpMessageHandlerBuilder.CreateHandlerPipeline(PrimaryHandler, AdditionalHandlers);
        }
    }
}
