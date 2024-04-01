using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CSharp.Net.Util.CsHttp
{
    internal class CsHttpClientFactory
    {
        private static readonly TimerCallback _cleanupCallback = delegate (object s)
        {
            ((CsHttpClientFactory)s).CleanupTimer_Tick();
        };
        private Timer _cleanupTimer;
        private readonly Func<string, Lazy<ActiveHandlerTrackingEntry>> _entryFactory;
        //IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor;
        private readonly object _cleanupTimerLock;
        private readonly List<IHttpMessageHandlerBuilderFilter> _filters;
        private readonly object _cleanupActiveLock;
        private readonly TimeSpan DefaultCleanupInterval = TimeSpan.FromSeconds(10.0);
        internal readonly ConcurrentDictionary<string, Lazy<ActiveHandlerTrackingEntry>> _activeHandlers;

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
            options = new HttpClientFactoryOptions();
        }

        public HttpClient CreateClient(string name = null)
        {
            name = name ?? string.Empty;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            HttpMessageHandler handler = CreateHandler(name);
            HttpClient httpClient = new HttpClient(handler, disposeHandler: false);
            //HttpClientFactoryOptions httpClientFactoryOptions = _optionsMonitor.Get(name);
            //for (int i = 0; i < httpClientFactoryOptions.HttpClientActions.Count; i++)
            //{
            //    httpClientFactoryOptions.HttpClientActions[i](httpClient);
            //}

            return httpClient;
        }

        public HttpMessageHandler CreateHandler(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

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
            catch
            {
                //serviceScope?.Dispose();
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
            // Log.HandlerExpired(_logger, activeHandlerTrackingEntry.Name, activeHandlerTrackingEntry.Lifetime);
            StartCleanupTimer();
        }

        internal virtual void StartHandlerEntryTimer(ActiveHandlerTrackingEntry entry)
        {
            entry.StartExpiryTimer(_expiryCallback);
        }

        internal virtual void StartCleanupTimer()
        {
            lock (_cleanupTimerLock)
            {
                if (_cleanupTimer == null)
                {
                    _cleanupTimer = NonCapturingTimer.Create(_cleanupCallback, this, DefaultCleanupInterval, Timeout.InfiniteTimeSpan);
                }
            }
        }

        internal virtual void StopCleanupTimer()
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
                //Log.CleanupCycleStart(_logger, count);
                ValueStopwatch valueStopwatch = ValueStopwatch.StartNew();
                int num = 0;
                for (int i = 0; i < count; i++)
                {
                    _expiredHandlers.TryDequeue(out ExpiredHandlerTrackingEntry result);
                    if (result.CanDispose)
                    {
                        try
                        {
                            result.InnerHandler.Dispose();
                            //result.Scope?.Dispose();
                            num++;
                        }
                        catch (Exception exception)
                        {
                            throw exception;
                            // Log.CleanupItemFailed(_logger, result.Name, exception);
                        }
                    }
                    else
                    {
                        _expiredHandlers.Enqueue(result);
                    }
                }

                //Log.CleanupCycleEnd(_logger, valueStopwatch.GetElapsedTime(), num, _expiredHandlers.Count);
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
