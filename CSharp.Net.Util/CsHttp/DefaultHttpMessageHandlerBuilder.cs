using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

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

        public override HttpMessageHandler PrimaryHandler { get; set; } = new HttpClientHandler();


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
