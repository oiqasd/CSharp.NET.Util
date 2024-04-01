using System.Net.Http;

namespace CSharp.Net.Util.CsHttp
{
    internal sealed class LifetimeTrackingHttpMessageHandler : DelegatingHandler
    {
        public LifetimeTrackingHttpMessageHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
