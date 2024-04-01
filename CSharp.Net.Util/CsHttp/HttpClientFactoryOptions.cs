using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CSharp.Net.Util.CsHttp
{
    internal class HttpClientFactoryOptions
    {
        internal static readonly TimeSpan MinimumHandlerLifetime = TimeSpan.FromSeconds(1.0);

        private TimeSpan _handlerLifetime = TimeSpan.FromMinutes(2.0);

        //
        // 摘要:
        //     Gets a list of operations used to configure an Microsoft.Extensions.Http.HttpMessageHandlerBuilder.
        public IList<Action<HttpMessageHandlerBuilder>> HttpMessageHandlerBuilderActions { get; } = new List<Action<HttpMessageHandlerBuilder>>();


        //
        // 摘要:
        //     Gets a list of operations used to configure an System.Net.Http.HttpClient.
        public IList<Action<HttpClient>> HttpClientActions { get; } = new List<Action<HttpClient>>();


        //
        // 摘要:
        //     Gets or sets the length of time that a System.Net.Http.HttpMessageHandler instance
        //     can be reused. Each named client can have its own configured handler lifetime
        //     value. The default value of this property is two minutes. Set the lifetime to
        //     System.Threading.Timeout.InfiniteTimeSpan to disable handler expiry.
        //
        // 言论：
        //     The default implementation of System.Net.Http.IHttpClientFactory will pool the
        //     System.Net.Http.HttpMessageHandler instances created by the factory to reduce
        //     resource consumption. This setting configures the amount of time a handler can
        //     be pooled before it is scheduled for removal from the pool and disposal.
        //
        //     Pooling of handlers is desirable as each handler typically manages its own underlying
        //     HTTP connections; creating more handlers than necessary can result in connection
        //     delays. Some handlers also keep connections open indefinitely which can prevent
        //     the handler from reacting to DNS changes. The value of Microsoft.Extensions.Http.HttpClientFactoryOptions.HandlerLifetime
        //     should be chosen with an understanding of the application's requirement to respond
        //     to changes in the network environment.
        //
        //     Expiry of a handler will not immediately dispose the handler. An expired handler
        //     is placed in a separate pool which is processed at intervals to dispose handlers
        //     only when they become unreachable. Using long-lived System.Net.Http.HttpClient
        //     instances will prevent the underlying System.Net.Http.HttpMessageHandler from
        //     being disposed until all references are garbage-collected.
        public TimeSpan HandlerLifetime
        {
            get
            {
                return _handlerLifetime;
            }
            set
            {
                if (value != Timeout.InfiniteTimeSpan && value < MinimumHandlerLifetime)
                {
                    throw new ArgumentException("HandlerLifetime_InvalidValue");
                }

                _handlerLifetime = value;
            }
        }

        //
        // 摘要:
        //     The System.Func`2 which determines whether to redact the HTTP header value before
        //     logging.
        public Func<string, bool> ShouldRedactHeaderValue { get; set; } = (string header) => false;


        //
        // 摘要:
        //     Gets or sets a value that determines whether the System.Net.Http.IHttpClientFactory
        //     will create a dependency injection scope when building an System.Net.Http.HttpMessageHandler.
        //     If false (default), a scope will be created, otherwise a scope will not be created.
        //
        //
        //     This option is provided for compatibility with existing applications. It is recommended
        //     to use the default setting for new applications.
        //
        // 言论：
        //     The System.Net.Http.IHttpClientFactory will (by default) create a dependency
        //     injection scope each time it creates an System.Net.Http.HttpMessageHandler. The
        //     created scope has the same lifetime as the message handler, and will be disposed
        //     when the message handler is disposed.
        //
        //     When operations that are part of Microsoft.Extensions.Http.HttpClientFactoryOptions.HttpMessageHandlerBuilderActions
        //     are executed they will be provided with the scoped System.IServiceProvider via
        //     Microsoft.Extensions.Http.HttpMessageHandlerBuilder.Services. This includes retrieving
        //     a message handler from dependency injection, such as one registered using Microsoft.Extensions.DependencyInjection.HttpClientBuilderExtensions.AddHttpMessageHandler``1(Microsoft.Extensions.DependencyInjection.IHttpClientBuilder).
        public bool SuppressHandlerScope { get; set; }
    }
}
