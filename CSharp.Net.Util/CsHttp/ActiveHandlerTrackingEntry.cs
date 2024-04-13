using System;
using System.Threading;

namespace CSharp.Net.Util.CsHttp
{
    internal sealed class ActiveHandlerTrackingEntry
    {
        private static readonly TimerCallback _timerCallback = delegate (object s)
        {
            ((ActiveHandlerTrackingEntry)s).Timer_Tick();
        };

        private readonly object _lock;

        private bool _timerInitialized;

        private Timer _timer;

        private TimerCallback _callback;

        public LifetimeTrackingHttpMessageHandler Handler { get; private set; }

        public TimeSpan Lifetime { get; }

        public string Name { get; }

        public ActiveHandlerTrackingEntry(string name, LifetimeTrackingHttpMessageHandler handler, TimeSpan lifetime)
        {
            Name = name;
            Handler = handler;
            Lifetime = lifetime;
            _lock = new object();
        }

        public void StartExpiryTimer(TimerCallback callback)
        {
            if (!(Lifetime == Timeout.InfiniteTimeSpan) && !Volatile.Read(ref _timerInitialized))
            {
                StartExpiryTimerSlow(callback);
            }
        }

        private void StartExpiryTimerSlow(TimerCallback callback)
        {
            lock (_lock)
            {
                if (!Volatile.Read(ref _timerInitialized))
                {
                    _callback = callback;
                    _timer = NonCapturingTimer.Create(_timerCallback, this, Lifetime, Timeout.InfiniteTimeSpan);
                    _timerInitialized = true;
                }
            }
        }

        private void Timer_Tick()
        {
            lock (_lock)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                    _callback(this);
                }
            }
        }
    }
}
