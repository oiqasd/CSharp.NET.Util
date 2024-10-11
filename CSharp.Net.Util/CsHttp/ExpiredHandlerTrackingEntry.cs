using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace CSharp.Net.Util.CsHttp
{
    internal sealed class ExpiredHandlerTrackingEntry
    {
        private readonly WeakReference _livenessTracker;

        public bool CanDispose => !_livenessTracker.IsAlive;

        public HttpMessageHandler InnerHandler { get; }

        public string Name { get; }

        public ExpiredHandlerTrackingEntry(ActiveHandlerTrackingEntry other)
        {
            Name = other.Name;
            _livenessTracker = new WeakReference(other.Handler);
            InnerHandler = other.Handler.InnerHandler;
        }
    }
}
