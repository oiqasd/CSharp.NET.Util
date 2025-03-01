using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace CSharp.Net.Util.CsHttp
{
    internal sealed class CsHttpClientFactory
    {
        private static readonly TimerCallback _cleanupCallback = delegate (object s)
        {
            ((CsHttpClientFactory)s).CleanupTimer_Tick();
        };
        private Timer _cleanupTimer;
        internal readonly ConcurrentDictionary<string, Lazy<ActiveHandlerTrackingEntry>> _activeHandlers;
        private readonly Func<string, Lazy<ActiveHandlerTrackingEntry>> _entryFactory;
        //IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor;
        private readonly object _cleanupTimerLock;
        private readonly List<IHttpMessageHandlerBuilderFilter> _filters;
        private readonly object _cleanupActiveLock;
        private readonly TimeSpan DefaultCleanupInterval = TimeSpan.FromSeconds(10.0);

        internal readonly ConcurrentQueue<ExpiredHandlerTrackingEntry> _expiredHandlers;
        internal readonly HttpClientFactoryOptions options;
        private readonly TimerCallback _expiryCallback;

        public CsHttpClientFactory()
        {
            _activeHandlers = new ConcurrentDictionary<string, Lazy<ActiveHandlerTrackingEntry>>(StringComparer.Ordinal);
            _entryFactory = (string name) => new Lazy<ActiveHandlerTrackingEntry>(() => CreateHandlerEntry(name), LazyThreadSafetyMode.ExecutionAndPublication);
            _expiredHandlers = new ConcurrentQueue<ExpiredHandlerTrackingEntry>();
            _expiryCallback = ExpiryTimer_Tick;
            _cleanupTimerLock = new object();
            _cleanupActiveLock = new object();
            _filters = new List<IHttpMessageHandlerBuilderFilter>();
            options = new HttpClientFactoryOptions() { HandlerLifetime = TimeSpan.FromMinutes(2) };

            //启用保活机制,默认保持连接超时2小时,保持连接间隔1秒
            //SocketError枚举:https://learn.microsoft.com/ZH-CN/dotnet/api/system.net.sockets.socketerror?view=net-8.0
            //ServicePointManager.SetTcpKeepAlive(true, 7200000, 1000);
            //设置最大连接数
            //ServicePointManager.DefaultConnectionLimit = 512;
        }

        public HttpClient CreateClient(int timeout = -1, bool connectionClose = false, string name = null)
        {
            //name = name ?? string.Empty;
            //if (name == null)
            //    throw new ArgumentNullException("name");

            /****
              #if NET5_0_OR_GREATER
                var handler = new SocketsHttpHandler
                {
                    ConnectCallback = async (context, cancellationToken) =>
                    {
                        Console.WriteLine($"Target Host: {context.DnsEndPoint.Host}");
                        Console.WriteLine($"Target Port: {context.DnsEndPoint.Port}");
                        var dnsEndPoint = context.DnsEndPoint;
                        // 解析 DNS 并获取 IP 地址
                        var addresses = await System.Net.Dns.GetHostAddressesAsync(dnsEndPoint.Host);
                        Console.WriteLine($"Resolved IP: {string.Join(",", addresses.Select(a => a.ToString()))}");
                        var endPoint = new IPEndPoint(addresses[0], context.DnsEndPoint.Port);
                        Console.WriteLine($"Attempting to connect to: {endPoint}");
                        // 连接到目标服务器
                        var socket = new System.Net.Sockets.Socket(System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                        //await socket.ConnectAsync(dnsEndPoint.Host, dnsEndPoint.Port);
                        await socket.ConnectAsync(endPoint);
                        Console.WriteLine($"Connected to: {endPoint}");
                        return new System.Net.Sockets.NetworkStream(socket, ownsSocket: true);
                    }
                };
              #else
                 HttpMessageHandler handler = CreateHandler(name);
              #endif
            */

            HttpMessageHandler handler = CreateHandler(name);
            HttpClient client = new HttpClient(handler, disposeHandler: false);
            //client.DefaultRequestHeaders.Clear();
            if (connectionClose)
                client.DefaultRequestHeaders.ConnectionClose = connectionClose;
            if (timeout > 0)
                client.Timeout = TimeSpan.FromSeconds(timeout);
            //client.DefaultRequestHeaders.Connection.Add("keep-alive");
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("ContentType", "application/json");

            //HttpClientFactoryOptions httpClientFactoryOptions = _optionsMonitor.Get(name);
            //for (int i = 0; i < httpClientFactoryOptions.HttpClientActions.Count; i++)
            //{
            //    httpClientFactoryOptions.HttpClientActions[i](httpClient);
            //}
            return client;
        }

        public HttpMessageHandler CreateHandler(string name)
        {
            //if (name == null)
            //    throw new ArgumentNullException("name");
            name = name ?? nameof(HttpClientUtil);
            LimitHelper.Case(name).Wait();
            ActiveHandlerTrackingEntry value = _activeHandlers.GetOrAdd(name, _entryFactory).Value;
            StartHandlerEntryTimer(value);
            return value.Handler;
        }

        internal ActiveHandlerTrackingEntry CreateHandlerEntry(string name)
        {
            //IServiceProvider provider = _services;
            //IServiceScope serviceScope = null;
            //HttpClientFactoryOptions options = _optionsMonitor.Get(name);

            //if (!options.SuppressHandlerScope)
            //{
            //    serviceScope = _scopeFactory.CreateScope();
            //    provider = serviceScope.ServiceProvider;
            //}
            try
            {
                //HttpMessageHandlerBuilder requiredService = provider.GetRequiredService<HttpMessageHandlerBuilder>();
                HttpMessageHandlerBuilder requiredService = new DefaultHttpMessageHandlerBuilder();
                requiredService.Name = name;
                Action<HttpMessageHandlerBuilder> action = Configure;
                for (int num = _filters.Count - 1; num >= 0; num--)
                {
                    action = _filters[num].Configure(action);
                }

                action(requiredService);
                LifetimeTrackingHttpMessageHandler handler = new LifetimeTrackingHttpMessageHandler(requiredService.Build());
                return new ActiveHandlerTrackingEntry(name, handler, options.HandlerLifetime);
            }
            catch (Exception ex)
            {
                //serviceScope?.Dispose();
                LogHelper.Fatal(ex);
                throw;
            }

            void Configure(HttpMessageHandlerBuilder b)
            {
                for (int i = 0; i < options.HttpMessageHandlerBuilderActions.Count; i++)
                {
                    options.HttpMessageHandlerBuilderActions[i](b);
                }
            }
        }

        internal void ExpiryTimer_Tick(object state)
        {
            ActiveHandlerTrackingEntry activeHandlerTrackingEntry = (ActiveHandlerTrackingEntry)state;
            Lazy<ActiveHandlerTrackingEntry> value;
            bool flag = _activeHandlers.TryRemove(activeHandlerTrackingEntry.Name, out value);
            ExpiredHandlerTrackingEntry item = new ExpiredHandlerTrackingEntry(activeHandlerTrackingEntry);
            _expiredHandlers.Enqueue(item);
            StartCleanupTimer();
        }
        /// <summary>
        /// 跟踪缓存过期时间
        /// </summary>
        /// <param name="entry"></param>
        internal void StartHandlerEntryTimer(ActiveHandlerTrackingEntry entry)
        {
            entry.StartExpiryTimer(_expiryCallback);
        }

        internal void StartCleanupTimer()
        {
            lock (_cleanupTimerLock)
            {
                if (_cleanupTimer == null)
                {
                    _cleanupTimer = NonCapturingTimer.Create(_cleanupCallback, this, DefaultCleanupInterval, Timeout.InfiniteTimeSpan);
                }
            }
        }

        internal void StopCleanupTimer()
        {
            lock (_cleanupTimerLock)
            {
                _cleanupTimer.Dispose();
                _cleanupTimer = null;
            }
        }
        internal void CleanupTimer_Tick()
        {
            StopCleanupTimer();
            if (!Monitor.TryEnter(_cleanupActiveLock))
            {
                StartCleanupTimer();
                return;
            }

            try
            {
                int count = _expiredHandlers.Count;
                for (int i = 0; i < count; i++)
                {
                    _expiredHandlers.TryDequeue(out ExpiredHandlerTrackingEntry result);
                    if (result.CanDispose)//对象已经被gc
                    {
                        try
                        {
                            //清理非托管资源
                            result.InnerHandler.Dispose();
                            //result.Scope?.Dispose();
                            //LogHelper.Debug("CsHttpClientFactory", "HttpMessageHandler is Disposed.");
                        }
                        catch (Exception exception)
                        {
                            LogHelper.Fatal(exception);
                            throw;
                        }
                    }
                    else
                    {
                        _expiredHandlers.Enqueue(result);
                    }
                }
                if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
                    LogHelper.Debug("CsHttpClientFactory", $"CleanupCycleBefore:{count},End:" + _expiredHandlers.Count);
            }
            finally
            {
                Monitor.Exit(_cleanupActiveLock);
            }

            if (!_expiredHandlers.IsEmpty)
            {
                StartCleanupTimer();
            }
        }
    }
}
